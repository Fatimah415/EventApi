using System.Text;
using EventApi.Data;
using EventApi.Middleware;
using EventApi.Repositories;
using EventApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Razor Pages for the browser-based admin panel.
builder.Services.AddRazorPages();

// Centralized exception handling: unhandled exceptions become RFC 7807
// ProblemDetails responses instead of leaking stack traces.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// EF Core DbContext (SQL Server / LocalDB). Registered as Scoped by default.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Scoped (one instance per HTTP request): a service that depends on the
// Scoped DbContext must NOT be a Singleton, or DI throws a captive-dependency
// error ("Cannot consume scoped service from singleton").
builder.Services.AddScoped<IUserService, UserService>();

// Events feature: repository + service.
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();

// Auth feature: repository + services.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Reporting: raw ADO.NET repository.
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Admin panel: repository + service.
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IAdminService, AdminService>();

// File upload feature.
// FileValidator is Singleton: it is stateless (no DB dependency) and its
// _fileSignatures dictionary is read-only after construction, so sharing one
// instance across all requests is safe and avoids repeated allocations.
builder.Services.AddSingleton<IFileValidator, FileValidator>();

// FileService is Scoped: it depends on IWebHostEnvironment (Singleton) and
// IFileValidator (Singleton), but Scoped is the safer default lifetime for
// services that perform I/O, as it aligns with the request lifetime and
// makes future dependencies (e.g. a Scoped DbContext) straightforward to add.
builder.Services.AddScoped<IFileService, FileService>();

// Weather API integration.
// AddHttpClient<TInterface, TImplementation> registers WeatherService as a Typed
// HttpClient — IHttpClientFactory manages the HttpClient lifecycle (pooled handlers,
// DNS recycling) so we never use "new HttpClient()".
// Polly policies are chained as delegating handlers in the HttpClient pipeline.
builder.Services.AddHttpClient<IWeatherService, WeatherService>()
    // Retry: up to 3 retries with exponential backoff (1s, 2s, 4s).
    // Only retries transient errors (5xx, 408, HttpRequestException).
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1))))
    // Circuit Breaker: after 5 consecutive transient failures, reject all requests
    // immediately for 30 seconds. Prevents hammering a downed upstream service.
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30)))
    // Timeout: cancel any single HTTP attempt that takes longer than 10 seconds.
    // This is the outermost policy — it caps the total time including retries.
    .AddPolicyHandler(Polly.Policy.TimeoutAsync<HttpResponseMessage>(
        TimeSpan.FromSeconds(10)));

// Authentication: two schemes run side-by-side.
// - JWT Bearer  → protects all /api/* controller endpoints.
// - Cookie      → protects Razor Pages /Admin/* (browsers cannot send Bearer headers).
// The default scheme is set to Cookie so that [Authorize] on Razor Pages redirects
// to the login page instead of returning 401 JSON.
builder.Services.AddAuthentication(options =>
{
    // Razor Pages default → Cookie (enables redirect to /Admin/Login).
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    // API controllers explicitly use [Authorize] with Bearer, or the
    // controller uses [Authorize] and the JWT middleware challenges directly.
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath  = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    // Secure cookie in production; allow HTTP in dev.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.")))
    };
});

builder.Services.AddAuthorization();

// OpenAPI + Swagger UI. The security definition adds an "Authorize" button so a
// JWT can be pasted once and sent on every protected request from the UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string schemeId = "Bearer";
    options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT (Swagger adds the 'Bearer ' prefix)."
    });
    // Attach the Bearer requirement only to [Authorize] endpoints, so Swagger
    // sends the header for protected operations and leaves public ones open.
    options.DocumentFilter<SecurityRequirementsDocumentFilter>();
});

var app = builder.Build();

// Must be first: wraps the whole pipeline so any unhandled exception is
// converted to a ProblemDetails response.
app.UseExceptionHandler();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enables serving static files from wwwroot (including wwwroot/uploads).
// Without this middleware, GET requests to /uploads/{filename} return 404
// even though the file physically exists on disk.
// Placed after UseHttpsRedirection and before UseAuthentication so that
// image URLs can be fetched by browsers without requiring a JWT — images
// are public assets identified only by their unguessable GUID filename.
app.UseStaticFiles();

// Order matters: authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map Razor Pages (admin panel at /Admin/*).
app.MapRazorPages();

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
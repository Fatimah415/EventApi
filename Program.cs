using System.Text;
using EventApi.Data;
using EventApi.Middleware;
using EventApi.Repositories;
using EventApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

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

// JWT Bearer authentication: validates the Authorization header on every request.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

// Order matters: authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
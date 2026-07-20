# AI Prompt Transcript — EventBoard API

This document captures the key prompts used with AI assistance during development. Prompts are paraphrased where they were multi-turn conversations; the intent and response are summarized.

---

## 1. File Upload Implementation

**Prompt:**
> Implement secure image upload for the Event model in this ASP.NET Core 8 API. Use IFormFile, save files to wwwroot/uploads with a GUID filename, validate extension, MIME type, and magic bytes. Make the service Scoped and the validator Singleton. Handle orphan cleanup if the DB write fails after the file is saved.

**AI Output (key decisions):**
- `FileValidator` as a Singleton (stateless, read-only `_fileSignatures` dictionary)
- `FileService` as Scoped (safe for future DbContext dependency)
- Extension allowlist: `.jpg`, `.jpeg`, `.png`
- MIME allowlist: `image/jpeg`, `image/png`
- Magic byte check: JPEG `FF D8 FF` variants, PNG `89 50 4E 47 0D 0A 1A 0A`
- GUID filename generation discards original name to prevent path traversal
- Path traversal guard in `DeleteImageAsync` using `Path.GetFullPath` + `StartsWith`
- Orphan cleanup: try/catch around `repository.AddAsync()` → delete orphan on failure
- Old image deleted only after successful DB update (consistency-first ordering)

---

## 2. Weather API + Polly Integration

**Prompt:**
> Add OpenWeatherMap current weather integration. Use IHttpClientFactory with AddHttpClient<IWeatherService>. Add Polly policies: exponential retry (3 retries), circuit breaker (5 failures → 30s break), and a 10-second timeout. Read the API key from IConfiguration, never hardcode it. Return a clean WeatherDto hiding internal JSON structure.

**AI Output (key decisions):**
- `AddHttpClient<IWeatherService, WeatherService>()` with three chained delegating handlers
- Retry: `WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)))` → 1s, 2s, 4s
- Circuit Breaker: `CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))`
- Timeout: `Polly.Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10))` as outermost handler
- Separate internal model `OpenWeatherMapResponse` (never exposed) → public `WeatherDto`
- 404 from OpenWeatherMap → `KeyNotFoundException` → GlobalExceptionHandler → 404 to client
- 401 → `InvalidOperationException` logged as Error (misconfigured key)

---

## 3. Admin Panel — REST API

**Prompt:**
> Add a protected admin panel to this existing ASP.NET Core 8 API. Use [Authorize(Roles = "Admin")]. Implement: list/create/update/delete events (reuse EventService), list users (no passwords), change user role, list all bookings with user and event info, and analytics endpoint returning bookings-per-day, per-month, per-event. Follow existing repository/service/DTO patterns. Use existing AdminRepository and AdminService.

**AI Output (key decisions):**
- Separate `AdminController` (API), `AdminRepository` (EF Core), `AdminService` (business rules)
- `AdminRepository.GetAllUsersAsync()` → `AdminUserDto` projection — `PasswordHash` never selected
- Role validation in `AdminService.UpdateUserRoleAsync()` uses `HashSet<string>{"Admin","User"}` → `ArgumentException` → 400
- Analytics: `GroupBy` on `BookedAt.Year/Month/Day` in DB, `DateOnly` projection done in-memory (EF can't translate `DateOnly` constructor to SQL)
- `BookingsPerEvent` reuses existing `BookingsPerEventDto` shape
- DI: `AddScoped<IAdminRepository, AdminRepository>()` and `AddScoped<IAdminService, AdminService>()`

---

## 4. Admin Panel — Razor Pages UI

**Prompt:**
> Add a Razor Pages admin panel to this existing API project. No separate React project. Pages needed: Login, Dashboard (analytics + charts), Events (table + delete), Users (table + role change), Bookings (table). Require Admin role. Reuse existing AdminService, EventService, AuthService. Use cookie auth for Razor Pages alongside existing JWT Bearer for API. Keep the UI simple and functional.

**AI Output (key decisions):**
- Dual auth: `DefaultScheme = Cookie`, API controllers specify `AuthenticationSchemes = "Bearer"` explicitly
- `Login.cshtml.cs` reuses `AuthService.LoginAsync()` (same BCrypt) → parses role from issued JWT → signs in with `HttpContext.SignInAsync(Cookie scheme)`
- `_AdminLayout.cshtml` shared layout with sidebar nav; inline CSS — no CDN dependencies
- Bar charts rendered server-side with `style="height:X%"` CSS — no JavaScript charting library required
- `FilterTable()` JS function for client-side search on all tables
- `[Authorize(Roles = Roles.Admin)]` on every Razor Page model class

---

## 5. Security Review and Gitignore

**Prompt:**
> Review the project for secrets committed to Git. Check .gitignore for appsettings.Development.json and wwwroot/. Fix any gaps.

**AI Output:**
- `appsettings.Development.json` contained OpenWeatherMap API key — not gitignored
- `wwwroot/` was commented out in `.gitignore` — uncommented to exclude uploaded files
- `appsettings.Development.json` added to `.gitignore`
- Key already appeared in local session; rotation recommended

---

## 6. EF Core Bug Fix

**Prompt (runtime error observed):**
> EF Core throws InvalidOperationException: LINQ expression could not be translated — `new DateOnly(g.Key.Year, g.Key.Month, g.Key.Day)` in GroupBy Select.

**AI Output:**
- EF Core SQL Server provider cannot translate `DateOnly` constructor in a server-side query
- Fix: project anonymous type `{ Year, Month, Day, Count }` in DB, then `.AsEnumerable()` / `.Select(x => new DateOnly(...))` in memory

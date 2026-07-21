# EventBoard API — Submission Notes

## Features Implemented

| # | Feature | Status |
|---|---------|--------|
| 1 | Event image upload with full validation | ✅ Complete |
| 2 | OpenWeatherMap integration via IHttpClientFactory + Polly | ✅ Complete |
| 3 | Admin Panel — Razor Pages browser UI | ✅ Complete |
| 4 | Admin Panel — REST API endpoints | ✅ Complete |
| 5 | File upload security review | ✅ Complete |
| 6 | Role-based authorization (JWT Bearer + Cookie) | ✅ Complete |
| 7 | AI usage documented | ✅ Complete |

---

## Architecture and Key Design Decisions

### Project Structure
```
EventApi/
├── Controllers/      # API controllers (JWT Bearer auth)
├── Data/             # EF Core DbContext + Fluent API configurations
├── Middleware/        # GlobalExceptionHandler (RFC 7807 ProblemDetails)
├── Migrations/       # EF Core migrations
├── Models/           # Entities + DTOs
├── Pages/            # Razor Pages (admin UI, Cookie auth)
│   ├── Admin/        # Dashboard, Events, Users, Bookings, Login pages
│   └── Shared/       # _AdminLayout.cshtml
├── Repositories/     # IEventRepository, IUserRepository, IAdminRepository, IReportRepository
└── Service/          # IEventService, IAuthService, IAdminService, IFileService, IWeatherService
```

### Dual Authentication Scheme
- **JWT Bearer** — protects all `/api/*` endpoints. Token issued at login, validated on every request.
- **Cookie** — protects `/Admin/*` Razor Pages. Issued after the Login page verifies credentials via the existing `AuthService` (same BCrypt check). Both schemes coexist: `DefaultScheme = Cookie`, API controllers explicitly set `AuthenticationSchemes = "Bearer"`.

### Repository Pattern
Every data access goes through an interface: `IEventRepository`, `IUserRepository`, `IAdminRepository`, `IReportRepository`. This decouples controllers/services from EF Core, enabling unit testing and future swaps.

### GlobalExceptionHandler
All exceptions are caught centrally (`IExceptionHandler`) and mapped to RFC 7807 `ProblemDetails` responses. No stack traces are leaked to clients. Client errors (400, 403, 404) log as `Warning`; server faults (500) log as `Error`.

---

## File Upload Security Review

| Control | Implementation |
|---------|---------------|
| Extension allowlist | `.jpg`, `.jpeg`, `.png` only (`FileValidator`) |
| MIME type check | `image/jpeg`, `image/png` only |
| Magic-byte validation | JPEG: `FF D8 FF` prefix; PNG: `89 50 4E 47 0D 0A 1A 0A` — prevents content spoofing |
| Maximum file size | 5 MB hard limit |
| Filename sanitization | Original name discarded; `Guid.NewGuid() + extension` prevents path traversal via filename |
| Upload directory | `wwwroot/uploads/` — served as static files, isolated from app code |
| Path traversal guard | `Path.GetFullPath` + `StartsWith(safeBoundary)` check before any delete |
| Orphan cleanup | If DB write fails after image save, the orphan file is deleted in the `catch` block |
| Old image cleanup | On event update/delete, the old file is deleted after successful DB operation |
| Secrets in Git | `appsettings.Development.json` is not committed (API key excluded). `.gitignore` updated. |

---

## Weather API and Polly Resilience

- `IHttpClientFactory` — `AddHttpClient<IWeatherService, WeatherService>()` manages HttpClient pooling and DNS refresh.
- **Retry policy**: up to 3 retries on transient errors (5xx, 408, network exceptions) with exponential backoff (1s, 2s, 4s).
- **Circuit Breaker**: after 5 consecutive transient failures, all requests are rejected for 30 seconds to prevent hammering a downed service.
- **Timeout**: 10-second cap per attempt (outermost policy, applies to full retry chain).
- API key read from `IConfiguration` at runtime — never hardcoded.

---

## Admin Panel Functionality

### REST API (`/api/admin/*`) — JWT Bearer required, Admin role
| Endpoint | Description |
|----------|-------------|
| `GET /api/admin/events` | List all events |
| `GET /api/admin/events/{id}` | Single event |
| `POST /api/admin/events` | Create event |
| `PUT /api/admin/events/{id}` | Update event |
| `DELETE /api/admin/events/{id}` | Delete event |
| `GET /api/admin/users` | List all users (no password hash) |
| `GET /api/admin/users/{id}` | Single user |
| `PATCH /api/admin/users/{id}/role` | Change user role |
| `GET /api/admin/bookings` | All bookings with user + event info |
| `GET /api/admin/analytics` | Dashboard summary + chart data |

### Razor Pages UI (`/Admin/*`) — Cookie auth, Admin role
| Route | Description |
|-------|-------------|
| `/Admin/Login` | Login page (reuses AuthService/BCrypt) |
| `/Admin` | Dashboard — stat cards, bar charts |
| `/Admin/Events` | Events table with delete |
| `/Admin/Users` | Users table with role change |
| `/Admin/Bookings` | Bookings table with status badges |

---

## Known Trade-offs (3-Hour Time Limit)

1. **No event create/edit form in the UI** — delete is supported; create/edit is available via Swagger/API only.
2. **No pagination** — all records loaded at once; acceptable for the dataset size.
3. **No booking status update UI** — bookings are read-only in the admin panel; status changes would require a new booking service method.
4. **Cookie auth for admin UI reuses JWT claims parsing** — not ideal for production; a dedicated cookie-only auth flow would be cleaner.
5. **No unit tests** — out of scope for the time limit.

---

## How AI Was Used

See `docs/ai-transcript.md` for detailed prompt history.

Key AI-assisted tasks:
- Designing the file upload security layer (extension + MIME + magic-byte + path traversal)
- Implementing IHttpClientFactory + Polly pipeline
- Designing the AdminRepository aggregate queries and analytics
- Building the Razor Pages admin panel with dual auth (Cookie + JWT)
- Security review and gap identification

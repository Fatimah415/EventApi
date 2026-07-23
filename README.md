# EventApi — Auth & Database Integration Assessment

This branch contains the **Daily Assessment: Auth & DB Integration** work only.
Admin-panel, file-upload, and testing assessments are intentionally excluded so
the review scope is clear.

## Assessment coverage

- Controller → Service → Repository architecture with dependency injection
- Registration and login using BCrypt password hashing
- JWT Bearer authentication with issuer, audience, signature, lifetime, and
  role validation
- `User` and `Admin` role-based authorization for event mutations
- SQL Server and EF Core code-first entities for users, events, categories,
  bookings, and favorites
- Fluent API relationships, delete behaviors, enum conversion, indexes, and
  deterministic seed data
- Parameterized raw ADO.NET bookings-per-event report
- Dedicated second migration that adds `IX_EventBookings_Status`

## Mentor feedback addressed

1. `EventBooking.Status` is indexed in
   `20260723072243_AddEventBookingStatusIndex`.
2. The submission is published from the dedicated
   `codex/auth-db-integration` branch with the PR title
   **Daily Assessment: Auth & DB Integration**.
3. The branch is based directly on `main` and contains only this assessment.

## Local setup

Requirements: .NET 8 SDK and SQL Server LocalDB.

```powershell
dotnet restore
dotnet user-secrets set "Jwt:Key" "replace-with-at-least-32-random-characters"
dotnet ef database update
dotnet run --launch-profile http
```

Swagger is available at `http://localhost:5289/swagger` in Development.

The signing key is deliberately not committed. You can alternatively set the
`Jwt__Key` environment variable. Never use the sample command's value in a
deployed environment.

To test Admin authorization, register a user and promote it locally:

```sql
UPDATE Users
SET Role = 'Admin'
WHERE Email = 'test@eventboard.com';
```

Then log in again so the new JWT contains the updated role claim.

## Key endpoints

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET | `/api/events` | Public |
| GET | `/api/events/{id}` | Public |
| POST | `/api/events` | Admin |
| PUT | `/api/events/{id}` | Admin |
| DELETE | `/api/events/{id}` | Admin |
| GET | `/api/reports/bookings-per-event?status=Confirmed` | Public |

Ready-to-run examples are in `EventApi.http`. AI usage is recorded in
[`docs/AI-Prompts-Auth-Db.md`](docs/AI-Prompts-Auth-Db.md).

# AI Prompt Record — Auth & DB Integration

AI was used to accelerate scaffolding, review security, generate the reporting
query, and respond to mentor feedback. Every generated change was reviewed,
built, and checked against the lesson requirements.

## Authentication and architecture

1. "Create an `IUserRepository` and EF Core `UserRepository` with asynchronous
   methods to add a user, find a user by email, and check whether an email
   exists. Keep database access out of controllers and services."
2. "Generate an `AuthService.RegisterAsync` method that normalizes the email,
   checks for duplicates, hashes the password with BCrypt, assigns the User
   role, and saves through `IUserRepository`."
3. "Generate `LoginAsync` so unknown emails and incorrect passwords return the
   same result, then issue a JWT containing user id, email, and role claims."
4. "Create a `JwtTokenService` that reads its signing key, issuer, audience, and
   expiry from configuration, uses HS256, and never hardcodes a production
   secret."
5. "Configure JWT Bearer middleware and Swagger JWT support. Protect event
   POST, PUT, and DELETE endpoints with the Admin role while keeping GET
   endpoints public."
6. "Review the authentication flow for password storage, account enumeration,
   token expiry, issuer/audience validation, role-claim mapping, and secret
   management."

## EF Core schema and reporting

7. "Design EF Core entities for categories, events, users, event bookings, and
   event favorites. Use an explicit favorite join entity with a timestamp and
   a composite key."
8. "Write Fluent API configurations for all relationships, delete behaviors,
   required fields, maximum lengths, enum-to-string conversion, unique email,
   and query-driven indexes."
9. "Generate deterministic seed data for five categories, ten events, three
   users, bookings, and favorites without runtime values in `HasData`."
10. "Create an EF Core migration for the Event Board schema and verify all
    tables, foreign keys, indexes, enum storage, and seed rows."
11. "Write a parameterized asynchronous ADO.NET query returning every event
    title and its booking count using LEFT JOIN and GROUP BY, with an optional
    booking-status parameter."

## Mentor-feedback correction and final review

12. "Add an index on `EventBooking.Status` because the reporting query filters
    by status, then generate a dedicated second EF Core migration."
13. "Separate Auth & DB work from admin-panel, file-upload, and testing work.
    Create a clearly named branch and PR based directly on main."
14. "Audit the clean assessment branch for architecture violations, invalid
    HTTP examples, unvalidated foreign keys, cancellation-token propagation,
    exception mapping, committed secrets, migration correctness, and missing
    submission documentation. Fix all in-scope findings and verify the build."

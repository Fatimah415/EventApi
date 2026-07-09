# CLAUDE.md

## Project Overview
This project is a .NET 8 Web API named EventApi.

## Coding Conventions
- Use PascalCase for classes, methods, and public properties.
- Use camelCase for local variables and parameters.
- All database and I/O operations should be asynchronous.
- Use the Async suffix for async methods.
- Return IActionResult from controllers.
- Use Dependency Injection instead of creating objects manually.

## Architecture
- Follow Controller → Service → Repository pattern.
- Keep business logic out of controllers.
- Write clean, readable, and maintainable code.

## Entity Framework Core
- Use SQL Server.
- Use LINQ Method Syntax.
- Use AsNoTracking() for read-only queries.
- Validate inputs before accessing the database.

## Error Handling
- Return appropriate HTTP status codes.
- Throw ArgumentException for invalid parameters.

## Style Preferences
- Avoid duplicate code.
- Keep methods small and focused.
- Use meaningful variable and method names.
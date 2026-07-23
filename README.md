# EventBoard API

This repository contains the ASP.NET Core 8 API for the EventBoard project, featuring an advanced admin panel, secure image uploads, third-party weather API integration with Polly resilience, and comprehensive REST API endpoints.

## Submission Documentation

For the detailed 10/10 assignment submission documents, please review:
- [Submission Notes & Architecture](docs/submission-notes.md) - Contains the full file upload security review, architecture overview, and known trade-offs.
- [AI Prompt Transcript](docs/ai-transcript.md) - Details the prompts used with the AI assistant during development.

## Getting Started

1. Clone the repository
2. Ensure you have the .NET 8 SDK installed.
3. Update `appsettings.Development.json` with your own `Jwt:Key` and `OpenWeatherMap:ApiKey`.
4. Run `dotnet ef database update` to apply migrations (using LocalDB by default).
5. Run `dotnet run` to start the application.
6. Visit `/swagger` to test the API directly or `/Admin/Login` to access the Razor Pages Admin UI.

*Note: For the admin panel, you can use the default developer credentials mentioned on the login page.*

## Weather integration

Configure the OpenWeatherMap key without committing it by setting an environment
variable:

```powershell
$env:OpenWeatherMap__ApiKey = "<YOUR_API_KEY>"
```

The public endpoint is `GET /api/events/{id}/weather`; it does not require a
Bearer token. Missing or invalid keys, provider errors, timeouts, network
failures, malformed responses, and an open circuit return a stable response
with `available: false` and a friendly message. A sanitized successful internal
response is available at
[`docs/weather-success-example.json`](docs/weather-success-example.json).

## User account status

Administrators can enable or disable accounts from the admin Users page or the
admin-only POST endpoints. Disabled users cannot log in; enabling the account
restores login access. Public registration always creates an active standard
user and cannot set account status.

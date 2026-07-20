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

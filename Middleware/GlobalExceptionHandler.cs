using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Middleware;

/// <summary>
/// Catches any unhandled exception, logs it, and returns an RFC 7807
/// ProblemDetails response so clients get a consistent error shape and no
/// stack-trace details leak out.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, type) = exception switch
        {
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Bad request.",
                "https://tools.ietf.org/html/rfc9110#section-15.5.1"),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                "https://tools.ietf.org/html/rfc9110#section-15.5.2"),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found.",
                "https://tools.ietf.org/html/rfc9110#section-15.5.5"),
            DbUpdateException => (
                StatusCodes.Status409Conflict,
                "Database conflict.",
                "https://tools.ietf.org/html/rfc9110#section-15.5.10"),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "https://tools.ietf.org/html/rfc9110#section-15.6.1")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Server error for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Client error {StatusCode} for {Method} {Path}: {Message}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = statusCode < StatusCodes.Status500InternalServerError
                ? exception.Message
                : null
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

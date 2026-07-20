using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Middleware;

/// <summary>
/// Catches any unhandled exception, logs it, and returns an RFC 7807
/// ProblemDetails response so clients get a consistent error shape and no
/// stack-trace details leak out.
///
/// Known exception types are mapped to meaningful HTTP status codes so that
/// controllers remain clean and free from repetitive try/catch blocks.
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
        // Map known exception types to the correct HTTP status codes.
        // Unrecognised exceptions fall through to 500.
        var (statusCode, title) = exception switch
        {
            // Client sent invalid data (e.g. bad image file, missing field).
            ArgumentException           => (StatusCodes.Status400BadRequest,   "Bad request."),

            // Authenticated user lacks permission for this resource.
            // 401 is NOT used here — the JWT middleware handles unauthenticated
            // requests before they ever reach application code.
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden,    "Forbidden."),

            // Requested resource does not exist.
            KeyNotFoundException        => (StatusCodes.Status404NotFound,     "Resource not found."),

            // Disk I/O failures (full disk, locked file, stream errors).
            // These are server-side infrastructure faults, not client errors.
            IOException                 => (StatusCodes.Status500InternalServerError, "A file storage error occurred."),

            // Anything else is an unexpected server fault.
            _                           => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        // Only log server-side faults as errors; client errors are warnings.
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled server error for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Client error {StatusCode} for {Method} {Path}: {Message}",
                statusCode,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            // Expose the exception message on client errors only —
            // never leak internal details on 500s.
            Detail = statusCode < StatusCodes.Status500InternalServerError
                        ? exception.Message
                        : null,
            Type   = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

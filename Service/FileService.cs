using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EventApi.Services;

public interface IFileService
{
    /// <summary>
    /// Validates and saves an uploaded image to the wwwroot/uploads directory.
    /// Returns the relative path to the saved file (e.g. /uploads/{guid}.jpg).
    /// Throws ArgumentException for invalid files.
    /// </summary>
    Task<string> SaveImageAsync(IFormFile file);

    /// <summary>
    /// Deletes a previously saved image from wwwroot/uploads.
    /// Safely ignores null/empty paths and missing files.
    /// Only deletes files that physically reside inside wwwroot/uploads
    /// to prevent path traversal attacks.
    /// IOException bubbles up to GlobalExceptionHandler as 500.
    /// </summary>
    Task DeleteImageAsync(string? imagePath);
}

public class FileService : IFileService
{
    private readonly IFileValidator _fileValidator;
    private readonly IWebHostEnvironment _environment;

    public FileService(IFileValidator fileValidator, IWebHostEnvironment environment)
    {
        _fileValidator = fileValidator;
        _environment = environment;
    }

    // -------------------------------------------------------------------------
    // Resolve and cache the uploads folder path.
    // -------------------------------------------------------------------------
    private string GetUploadsFolder()
    {
        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        return Path.Combine(webRootPath, "uploads");
    }

    // -------------------------------------------------------------------------
    // SaveImageAsync
    // -------------------------------------------------------------------------
    public async Task<string> SaveImageAsync(IFormFile file)
    {
        // 1. Validate — ArgumentException bubbles to GlobalExceptionHandler → 400.
        var (isValid, errorMessage) = _fileValidator.ValidateImage(file);
        if (!isValid)
            throw new ArgumentException(errorMessage);

        // 2. Ensure the uploads directory exists.
        var uploadsFolder = GetUploadsFolder();
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // 3. Generate a secure, unique filename discarding the original name.
        var extension      = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath       = Path.Combine(uploadsFolder, uniqueFileName);

        // 4. Save asynchronously.
        //    IOException bubbles to GlobalExceptionHandler → 500.
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // 5. Return the relative URL path for database storage and client use.
        return $"/uploads/{uniqueFileName}";
    }

    // -------------------------------------------------------------------------
    // DeleteImageAsync
    // -------------------------------------------------------------------------
    public Task DeleteImageAsync(string? imagePath)
    {
        // Guard 1: Nothing to delete if the path is null or empty.
        if (string.IsNullOrWhiteSpace(imagePath))
            return Task.CompletedTask;

        var uploadsFolder = GetUploadsFolder();

        // Guard 2: Reconstruct the absolute physical path safely.
        // The imagePath from the DB usually looks like "/uploads/filename.jpg".
        // We safely map this URL path to a relative file path.
        var relativePath = imagePath;
        if (relativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath.Substring("/uploads/".Length);
        }
        else if (relativePath.StartsWith("\\uploads\\", StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath.Substring("\\uploads\\".Length);
        }

        // Remove any leading slashes to prevent Path.Combine from treating it as an absolute rooted path.
        relativePath = relativePath.TrimStart('/', '\\');

        var absolutePath = Path.GetFullPath(Path.Combine(uploadsFolder, relativePath));

        // Guard 3: Path traversal & prefix attack prevention.
        var safeBoundary = Path.GetFullPath(uploadsFolder);

        // Ensure the boundary ends with a directory separator to prevent prefix attacks
        // (e.g. C:\wwwroot\uploads_evil matching C:\wwwroot\uploads)
        if (!safeBoundary.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            safeBoundary += Path.DirectorySeparatorChar;
        }

        if (!absolutePath.StartsWith(safeBoundary, StringComparison.OrdinalIgnoreCase))
        {
            // The path resolved outside the uploads directory. Throw an exception to flag the attack.
            throw new InvalidOperationException(
                $"Attempted to delete a file outside the uploads directory: {absolutePath}");
        }

        // Guard 4: Silently skip deletion if the file no longer exists.
        if (!File.Exists(absolutePath))
            return Task.CompletedTask;

        File.Delete(absolutePath);
        return Task.CompletedTask;
    }
}

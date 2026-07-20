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

        // Guard 2: Reconstruct the absolute physical path from the relative path
        //          stored in the database (e.g. /uploads/{guid}.jpg → C:\...\wwwroot\uploads\{guid}.jpg).
        var fileName     = Path.GetFileName(imagePath); // strips any directory portion
        var absolutePath = Path.GetFullPath(Path.Combine(uploadsFolder, fileName));

        // Guard 3: Path traversal prevention.
        //          Ensure the resolved path is still inside wwwroot/uploads.
        //          Path.GetFullPath normalises ".." segments, so comparing with
        //          StartsWith catches any attempt to escape the uploads directory.
        var safeBoundary = Path.GetFullPath(uploadsFolder);
        if (!absolutePath.StartsWith(safeBoundary, StringComparison.OrdinalIgnoreCase))
        {
            // This is a server-side logic error, not a client mistake — 500 is correct.
            throw new InvalidOperationException(
                $"Attempted to delete a file outside the uploads directory: {absolutePath}");
        }

        // Guard 4: Silently skip deletion if the file no longer exists
        //          (e.g. was manually removed, or the earlier save failed).
        if (!File.Exists(absolutePath))
            return Task.CompletedTask;

        // Delete the file; IOException bubbles to GlobalExceptionHandler → 500.
        File.Delete(absolutePath);
        return Task.CompletedTask;
    }
}

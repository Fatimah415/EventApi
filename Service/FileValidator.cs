using System.IO;
using Microsoft.AspNetCore.Http;

namespace EventApi.Services;

public interface IFileValidator
{
    (bool IsValid, string ErrorMessage) ValidateImage(IFormFile? file);
}

public class FileValidator : IFileValidator
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png" };
    private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    // Magic bytes (file signatures) for JPEG and PNG
    private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
    {
        { ".jpeg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }
            }
        },
        { ".jpg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xEE },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }
            }
        },
        { ".png", new List<byte[]>
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        }
    };

    public (bool IsValid, string ErrorMessage) ValidateImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File is empty or not provided.");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            return (false, $"File size exceeds the maximum limit of {MaxFileSizeInBytes / (1024 * 1024)}MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            return (false, "Invalid file extension. Only .jpg, .jpeg, and .png are allowed.");
        }

        if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return (false, "Invalid MIME type. Only image/jpeg and image/png are allowed.");
        }

        if (!IsValidFileSignature(file, extension))
        {
            return (false, "File signature (magic bytes) validation failed. The file might be corrupted or spoofed.");
        }

        return (true, string.Empty);
    }

    private bool IsValidFileSignature(IFormFile file, string extension)
    {
        var stream = file.OpenReadStream();
        // Use leaveOpen: true so the underlying stream is not closed when the BinaryReader is disposed.
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        
        var signatures = _fileSignatures[extension];
        var maxSignatureLength = signatures.Max(m => m.Length);
        
        var headerBytes = reader.ReadBytes(maxSignatureLength);
        
        // Reset stream position so it can be read again later during the actual save
        stream.Position = 0;

        return signatures.Any(signature => 
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }
}

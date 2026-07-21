using EventApi.Services;
using Microsoft.AspNetCore.Http;

namespace EventApi.Tests;

public class FileValidatorTests
{
    private readonly FileValidator _validator = new();

    [Fact]
    public void ValidateImage_NullFile_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidateImage(null);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("File is empty or not provided.", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_EmptyFile_ReturnsInvalid()
    {
        // Arrange
        var file = CreateFormFile(
            Array.Empty<byte>(),
            "image.jpg",
            "image/jpeg");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("File is empty or not provided.", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_FileTooLarge_ReturnsInvalid()
    {
        // Arrange
        var data = new byte[(5 * 1024 * 1024) + 1];

        var file = CreateFormFile(
            data,
            "large.jpg",
            "image/jpeg");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("File size exceeds", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_InvalidExtension_ReturnsInvalid()
    {
        // Arrange
        var file = CreateFormFile(
            new byte[] { 1, 2, 3, 4 },
            "image.gif",
            "image/gif");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid file extension", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_InvalidMimeType_ReturnsInvalid()
    {
        // Arrange
        var jpegSignature = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0
        };

        var file = CreateFormFile(
            jpegSignature,
            "image.jpg",
            "application/octet-stream");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid MIME type", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_InvalidMagicBytes_ReturnsInvalid()
    {
        // Arrange
        var fakeImage = new byte[]
        {
            0x01, 0x02, 0x03, 0x04
        };

        var file = CreateFormFile(
            fakeImage,
            "image.jpg",
            "image/jpeg");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("magic bytes", result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_ValidJpeg_ReturnsValid()
    {
        // Arrange
        var jpegSignature = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0
        };

        var file = CreateFormFile(
            jpegSignature,
            "image.jpg",
            "image/jpeg");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_ValidJpegExtension_ReturnsValid()
    {
        // Arrange
        var jpegSignature = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0
        };

        var file = CreateFormFile(
            jpegSignature,
            "image.jpeg",
            "image/jpeg");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    [Fact]
    public void ValidateImage_ValidPng_ReturnsValid()
    {
        // Arrange
        var pngSignature = new byte[]
        {
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A
        };

        var file = CreateFormFile(
            pngSignature,
            "image.png",
            "image/png");

        // Act
        var result = _validator.ValidateImage(file);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.ErrorMessage);
    }

    private static IFormFile CreateFormFile(
        byte[] content,
        string fileName,
        string contentType)
    {
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
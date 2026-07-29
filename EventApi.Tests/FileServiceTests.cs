using EventApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;

namespace EventApi.Tests;

public class FileServiceTests : IDisposable
{
    private readonly string _testWebRoot;
    private readonly Mock<IFileValidator> _validatorMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _testWebRoot = Path.Combine(
            Path.GetTempPath(),
            "EventApiTests",
            Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testWebRoot);

        _validatorMock = new Mock<IFileValidator>();
        _environmentMock = new Mock<IWebHostEnvironment>();

        _environmentMock
            .Setup(x => x.WebRootPath)
            .Returns(_testWebRoot);

        _fileService = new FileService(
            _validatorMock.Object,
            _environmentMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testWebRoot))
        {
            Directory.Delete(_testWebRoot, recursive: true);
        }
    }

    // ============================================================
    // SaveImageAsync Tests
    // ============================================================

    [Fact]
    public async Task SaveImageAsync_ValidImage_SavesFileAndReturnsPath()
    {
        // Arrange
        var imageBytes = CreateJpegBytes();

        var file = CreateFormFile(
            imageBytes,
            "test.jpg",
            "image/jpeg");

        _validatorMock
            .Setup(x => x.ValidateImage(file))
            .Returns((true, string.Empty));

        // Act
        var result = await _fileService.SaveImageAsync(file);

        // Assert
        result.Should().StartWith("/uploads/");
        result.Should().EndWith(".jpg");

        var fileName = Path.GetFileName(result);
        var savedFilePath = Path.Combine(
            _testWebRoot,
            "uploads",
            fileName);

        File.Exists(savedFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveImageAsync_UploadsDirectoryDoesNotExist_CreatesDirectory()
    {
        // Arrange
        var imageBytes = CreateJpegBytes();

        var file = CreateFormFile(
            imageBytes,
            "test.jpg",
            "image/jpeg");

        _validatorMock
            .Setup(x => x.ValidateImage(file))
            .Returns((true, string.Empty));

        var uploadsFolder = Path.Combine(
            _testWebRoot,
            "uploads");

        Directory.Exists(uploadsFolder).Should().BeFalse();

        // Act
        await _fileService.SaveImageAsync(file);

        // Assert
        Directory.Exists(uploadsFolder).Should().BeTrue();
    }

    [Fact]
    public async Task SaveImageAsync_InvalidFile_ThrowsArgumentException()
    {
        // Arrange
        var file = CreateFormFile(
            new byte[] { 1, 2, 3 },
            "malware.exe",
            "application/octet-stream");

        _validatorMock
            .Setup(x => x.ValidateImage(file))
            .Returns((false, "Invalid file."));

        // Act & Assert
        Func<Task> action = () => _fileService.SaveImageAsync(file);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Invalid file.");
    }

    [Fact]
    public async Task SaveImageAsync_PngFile_ReturnsPngPath()
    {
        // Arrange
        var file = CreateFormFile(
            CreatePngBytes(),
            "image.png",
            "image/png");

        _validatorMock
            .Setup(x => x.ValidateImage(file))
            .Returns((true, string.Empty));

        // Act
        var result = await _fileService.SaveImageAsync(file);

        // Assert
        result.Should().EndWith(".png");
    }

    // ============================================================
    // DeleteImageAsync Tests
    // ============================================================

    [Fact]
    public async Task DeleteImageAsync_NullPath_DoesNothing()
    {
        var uploadsFolder = Path.Combine(_testWebRoot, "uploads");

        await _fileService.DeleteImageAsync(null);

        Directory.Exists(uploadsFolder).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_EmptyPath_DoesNothing()
    {
        var uploadsFolder = Path.Combine(_testWebRoot, "uploads");

        await _fileService.DeleteImageAsync("");

        Directory.Exists(uploadsFolder).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_WhitespacePath_DoesNothing()
    {
        var uploadsFolder = Path.Combine(_testWebRoot, "uploads");

        await _fileService.DeleteImageAsync("   ");

        Directory.Exists(uploadsFolder).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_ExistingFile_DeletesFile()
    {
        // Arrange
        var uploadsFolder = Path.Combine(
            _testWebRoot,
            "uploads");

        Directory.CreateDirectory(uploadsFolder);

        var fileName = "test-image.jpg";
        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        await File.WriteAllBytesAsync(
            filePath,
            CreateJpegBytes());

        // Act
        await _fileService.DeleteImageAsync(
            $"/uploads/{fileName}");

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_FileDoesNotExist_DoesNothing()
    {
        var uploadsFolder = Path.Combine(_testWebRoot, "uploads");

        await _fileService.DeleteImageAsync(
            "/uploads/missing.jpg");

        Directory.Exists(uploadsFolder).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_PathTraversal_ThrowsInvalidOperationException()
    {
        Func<Task> action = () => _fileService.DeleteImageAsync(
            "/uploads/../../secret.txt");

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    // ============================================================
    // Helper Methods
    // ============================================================

    private static FormFile CreateFormFile(
        byte[] content,
        string fileName,
        string contentType)
    {
        var stream = new MemoryStream(content);

        return new FormFile(
            stream,
            0,
            content.Length,
            "file",
            fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static byte[] CreateJpegBytes()
    {
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0,
            0x01, 0x02, 0x03, 0x04
        };
    }

    private static byte[] CreatePngBytes()
    {
        return new byte[]
        {
            0x89, 0x50, 0x4E,
            0x47, 0x0D, 0x0A,
            0x1A, 0x0A,
            0x01, 0x02
        };
    }
}

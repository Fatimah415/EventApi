using EventApi.Models;
using EventApi.Repositories;
using EventApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventApi.Tests;

public class EventServiceTests
{
    private readonly Mock<IEventRepository> _repositoryMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<ILogger<EventService>> _loggerMock;
    private readonly EventService _sut; // System Under Test

    public EventServiceTests()
    {
        _repositoryMock = new Mock<IEventRepository>();
        _fileServiceMock = new Mock<IFileService>();
        _loggerMock = new Mock<ILogger<EventService>>();
        
        _sut = new EventService(
            _repositoryMock.Object,
            _fileServiceMock.Object,
            _loggerMock.Object);
    }

    // ---------------------------------------------------------
    // GetAllAsync Tests
    // ---------------------------------------------------------
    [Fact]
    public async Task GetAllAsync_ReturnsMappedEvents()
    {
        // Arrange
        var events = new List<Event>
        {
            new Event { Id = 1, Title = "Event 1" },
            new Event { Id = 2, Title = "Event 2" }
        };
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(events);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Event 1");
    }

    // ---------------------------------------------------------
    // GetByIdAsync Tests
    // ---------------------------------------------------------
    [Fact]
    public async Task GetByIdAsync_EventExists_ReturnsMappedEvent()
    {
        // Arrange
        var ev = new Event { Id = 1, Title = "Event 1" };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(ev);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Should().BeEquivalentTo(new { Id = 1, Title = "Event 1" });
    }

    [Fact]
    public async Task GetByIdAsync_EventDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Event?)null);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    // ---------------------------------------------------------
    // CreateAsync Tests
    // ---------------------------------------------------------
    [Fact]
    public async Task CreateAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dto = ValidCreateDto(userId: 99);
        _repositoryMock.Setup(repo => repo.UserExistsAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidDtoWithoutImage_AddsToRepository()
    {
        // Arrange
        var dto = ValidCreateDto();
        _repositoryMock.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test");
        result.ImagePath.Should().BeNull();
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(It.IsAny<IFormFile>()), Times.Never);
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Event>(e => e.Title == "Test")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidDtoWithImage_SavesImageAndAddsToRepository()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var dto = ValidCreateDto(image: mockFile.Object);
        
        _repositoryMock.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("image.jpg");

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.ImagePath.Should().Be("image.jpg");
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(mockFile.Object), Times.Once);
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Event>(e => e.ImagePath == "image.jpg")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrowsWithImage_CleansUpOrphanedImage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var dto = ValidCreateDto(image: mockFile.Object);
        
        _repositoryMock.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("image.jpg");
        _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Event>())).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        Func<Task> action = () => _sut.CreateAsync(dto);

        await action.Should().ThrowAsync<Exception>().WithMessage("DB Error");
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(mockFile.Object), Times.Once);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync("image.jpg"), Times.Once);
    }

    // ---------------------------------------------------------
    // UpdateAsync Tests
    // ---------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_EventDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Event?)null);

        // Act
        var result = await _sut.UpdateAsync(1, ValidUpdateDto());

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithoutImage_UpdatesFieldsCorrectly()
    {
        // Arrange
        var existing = new Event { Id = 1, Title = "Old", ImagePath = "old.jpg" };
        var dto = ValidUpdateDto();
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.Should().BeTrue();
        existing.Title.Should().Be("New");
        existing.ImagePath.Should().Be("old.jpg"); // Preserved
        
        _repositoryMock.Verify(repo => repo.UpdateAsync(existing), Times.Once);
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(It.IsAny<IFormFile>()), Times.Never);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithNewImage_SavesNewAndDeletesOld()
    {
        // Arrange
        var existing = new Event { Id = 1, Title = "Old", ImagePath = "old.jpg" };
        var mockFile = new Mock<IFormFile>();
        var dto = ValidUpdateDto(mockFile.Object);
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("new.jpg");

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.Should().BeTrue();
        existing.Title.Should().Be("New");
        existing.ImagePath.Should().Be("new.jpg");
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(mockFile.Object), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateAsync(existing), Times.Once);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync("old.jpg"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RepositoryThrowsWithNewImage_CleansUpOrphanedNewImage()
    {
        // Arrange
        var existing = new Event { Id = 1, Title = "Old", ImagePath = "old.jpg" };
        var mockFile = new Mock<IFormFile>();
        var dto = ValidUpdateDto(mockFile.Object);
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("new.jpg");
        _repositoryMock.Setup(repo => repo.UpdateAsync(existing)).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        Func<Task> action = () => _sut.UpdateAsync(1, dto);

        await action.Should().ThrowAsync<Exception>().WithMessage("DB Error");
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(mockFile.Object), Times.Once);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync("new.jpg"), Times.Once); // The newly saved image
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync("old.jpg"), Times.Never); // The old image remains intact
    }

    // ---------------------------------------------------------
    // DeleteAsync Tests
    // ---------------------------------------------------------
    [Fact]
    public async Task DeleteAsync_EventDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Event?)null);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithoutImage_DeletesEventOnly()
    {
        // Arrange
        var existing = new Event { Id = 1, ImagePath = null };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.DeleteAsync(existing), Times.Once);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithImage_DeletesEventAndImage()
    {
        // Arrange
        var existing = new Event { Id = 1, ImagePath = "image.jpg" };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.DeleteAsync(existing), Times.Once);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync("image.jpg"), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_InvalidId_ThrowsArgumentException(int id)
    {
        Func<Task> action = () => _sut.GetByIdAsync(id);

        await action.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
    {
        Func<Task> action = () => _sut.CreateAsync(null!);

        await action.Should().ThrowAsync<ArgumentNullException>();
        _repositoryMock.Verify(repo => repo.UserExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_MissingTitle_ThrowsArgumentException(string title)
    {
        var dto = ValidCreateDto();
        dto.Title = title;

        Func<Task> action = () => _sut.CreateAsync(dto);

        await action.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(repo => repo.UserExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData("", 1, 1)]
    [InlineData("   ", 1, 1)]
    [InlineData("Lahore", 0, 1)]
    [InlineData("Lahore", 1, 0)]
    public async Task CreateAsync_InvalidForeignOrLocationField_ThrowsArgumentException(
        string location,
        int categoryId,
        int userId)
    {
        var dto = ValidCreateDto(userId);
        dto.Location = location;
        dto.CategoryId = categoryId;

        Func<Task> action = () => _sut.CreateAsync(dto);

        await action.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(repo => repo.UserExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
    {
        Func<Task> action = () => _sut.UpdateAsync(1, null!);

        await action.Should().ThrowAsync<ArgumentNullException>();
        _repositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OldImageDeleteFails_ReturnsSuccess()
    {
        var existing = new Event { Id = 1, Title = "Old", ImagePath = "old.jpg" };
        var mockFile = new Mock<IFormFile>();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);
        _fileServiceMock.Setup(service => service.SaveImageAsync(mockFile.Object)).ReturnsAsync("new.jpg");
        _fileServiceMock.Setup(service => service.DeleteImageAsync("old.jpg")).ThrowsAsync(new IOException());

        var result = await _sut.UpdateAsync(1, ValidUpdateDto(mockFile.Object));

        result.Should().BeTrue();
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task DeleteAsync_ImageDeleteFails_ReturnsSuccess()
    {
        var existing = new Event { Id = 1, ImagePath = "image.jpg" };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);
        _fileServiceMock.Setup(service => service.DeleteImageAsync("image.jpg")).ThrowsAsync(new IOException());

        var result = await _sut.DeleteAsync(1);

        result.Should().BeTrue();
        _loggerMock.VerifyLog(LogLevel.Warning, Times.Once());
    }

    private static CreateEventDto ValidCreateDto(int userId = 1, IFormFile? image = null) => new()
    {
        Title = "Test",
        Location = "Lahore",
        EventDate = DateTime.UtcNow.AddDays(1),
        CategoryId = 1,
        UserId = userId,
        Image = image
    };

    private static UpdateEventDto ValidUpdateDto(IFormFile? image = null) => new()
    {
        Title = "New",
        Location = "Karachi",
        EventDate = DateTime.UtcNow.AddDays(2),
        CategoryId = 2,
        Image = image
    };
}

internal static class LoggerMockExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, Times times) =>
        loggerMock.Verify(
            logger => logger.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
}

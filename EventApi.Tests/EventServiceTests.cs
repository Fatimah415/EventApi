using EventApi.Models;
using EventApi.Repositories;
using EventApi.Services;
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
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Event 1", result.First().Title);
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
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Event 1", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_EventDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Event?)null);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    // ---------------------------------------------------------
    // CreateAsync Tests
    // ---------------------------------------------------------
    [Fact]
    public async Task CreateAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dto = new CreateEventDto { UserId = 99 };
        _repositoryMock.Setup(repo => repo.UserExistsAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.Null(result);
        _repositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidDtoWithoutImage_AddsToRepository()
    {
        // Arrange
        var dto = new CreateEventDto { UserId = 1, Title = "Test" };
        _repositoryMock.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
        Assert.Null(result.ImagePath);
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(It.IsAny<IFormFile>()), Times.Never);
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Event>(e => e.Title == "Test")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidDtoWithImage_SavesImageAndAddsToRepository()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var dto = new CreateEventDto { UserId = 1, Title = "Test", Image = mockFile.Object };
        
        _repositoryMock.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("image.jpg");

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("image.jpg", result.ImagePath);
        
        _fileServiceMock.Verify(fs => fs.SaveImageAsync(mockFile.Object), Times.Once);
        _repositoryMock.Verify(repo => repo.AddAsync(It.Is<Event>(e => e.ImagePath == "image.jpg")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrowsWithImage_CleansUpOrphanedImage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var dto = new CreateEventDto { UserId = 1, Title = "Test", Image = mockFile.Object };
        
        _repositoryMock.Setup(repo => repo.UserExistsAsync(1)).ReturnsAsync(true);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("image.jpg");
        _repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Event>())).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
        
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
        var result = await _sut.UpdateAsync(1, new UpdateEventDto());

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Event>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithoutImage_UpdatesFieldsCorrectly()
    {
        // Arrange
        var existing = new Event { Id = 1, Title = "Old", ImagePath = "old.jpg" };
        var dto = new UpdateEventDto { Title = "New" };
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        Assert.True(result);
        Assert.Equal("New", existing.Title);
        Assert.Equal("old.jpg", existing.ImagePath); // Preserved
        
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
        var dto = new UpdateEventDto { Title = "New", Image = mockFile.Object };
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("new.jpg");

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        Assert.True(result);
        Assert.Equal("New", existing.Title);
        Assert.Equal("new.jpg", existing.ImagePath);
        
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
        var dto = new UpdateEventDto { Title = "New", Image = mockFile.Object };
        
        _repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existing);
        _fileServiceMock.Setup(fs => fs.SaveImageAsync(mockFile.Object)).ReturnsAsync("new.jpg");
        _repositoryMock.Setup(repo => repo.UpdateAsync(existing)).ThrowsAsync(new Exception("DB Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(1, dto));
        
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
        Assert.False(result);
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
        Assert.True(result);
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
        Assert.True(result);
        _repositoryMock.Verify(repo => repo.DeleteAsync(existing), Times.Once);
        _fileServiceMock.Verify(fs => fs.DeleteImageAsync("image.jpg"), Times.Once);
    }
}

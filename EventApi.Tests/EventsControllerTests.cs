using EventApi.Controllers;
using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventApi.Tests;

public class EventsControllerTests
{
    private readonly Mock<IEventService> _eventServiceMock = new();
    private readonly Mock<IWeatherService> _weatherServiceMock = new();

    private EventsController CreateController() =>
        new(_eventServiceMock.Object, _weatherServiceMock.Object);

    [Fact]
    public async Task GetWeather_MissingEvent_ReturnsNotFound()
    {
        _eventServiceMock.Setup(service => service.GetByIdAsync(10)).ReturnsAsync((EventResponseDto?)null);

        var result = await CreateController().GetWeather(10);

        Assert.IsType<NotFoundObjectResult>(result);
        _weatherServiceMock.Verify(
            service => service.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetWeather_MissingLocation_ReturnsBadRequest(string location)
    {
        _eventServiceMock.Setup(service => service.GetByIdAsync(10)).ReturnsAsync(
            new EventResponseDto { Id = 10, Location = location });

        var result = await CreateController().GetWeather(10);

        Assert.IsType<BadRequestObjectResult>(result);
        _weatherServiceMock.Verify(
            service => service.GetWeatherAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWeather_ValidEvent_ReturnsWeather()
    {
        var weather = new WeatherDto { City = "Lahore", TemperatureCelsius = 31 };
        _eventServiceMock.Setup(service => service.GetByIdAsync(10)).ReturnsAsync(
            new EventResponseDto { Id = 10, Location = "Lahore" });
        _weatherServiceMock.Setup(service => service.GetWeatherAsync("Lahore", It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        var result = await CreateController().GetWeather(10);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(weather, ok.Value);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsBadRequestWithoutCallingService()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Title", "Title is required.");

        var result = await controller.Create(new CreateEventDto());

        Assert.IsType<BadRequestObjectResult>(result);
        _eventServiceMock.Verify(service => service.CreateAsync(It.IsAny<CreateEventDto>()), Times.Never);
    }

    [Fact]
    public async Task Create_MissingOwner_ReturnsBadRequest()
    {
        var dto = new CreateEventDto { UserId = 999, Title = "Event", Location = "Lahore", CategoryId = 1 };
        _eventServiceMock.Setup(service => service.CreateAsync(dto)).ReturnsAsync((EventResponseDto?)null);

        var result = await CreateController().Create(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("does not exist", badRequest.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Update_InvalidModelState_ReturnsBadRequestWithoutCallingService()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("Location", "Location is required.");

        var result = await controller.Update(1, new UpdateEventDto());

        Assert.IsType<BadRequestObjectResult>(result);
        _eventServiceMock.Verify(
            service => service.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateEventDto>()),
            Times.Never);
    }
}

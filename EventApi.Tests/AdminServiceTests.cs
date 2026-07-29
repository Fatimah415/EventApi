using EventApi.Models;
using EventApi.Repositories;
using EventApi.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventApi.Tests;

public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _repositoryMock = new();
    private readonly Mock<ILogger<AdminService>> _loggerMock = new();
    private readonly AdminService _sut;

    public AdminServiceTests()
    {
        _sut = new AdminService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_RepositoryReturnsUsers_ReturnsThoseDtos()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var users = new List<AdminUserDto>
        {
            new() { Id = 1, Name = "Aisha", Email = "aisha@example.com", Role = Roles.Admin, BookingCount = 2 },
            new() { Id = 2, Name = "Bilal", Email = "bilal@example.com", Role = Roles.User, BookingCount = 1 }
        };
        _repositoryMock
            .Setup(repository => repository.GetAllUsersAsync(cancellationToken))
            .ReturnsAsync(users);

        var result = await _sut.GetAllUsersAsync(cancellationToken);

        result.Should().BeSameAs(users);
        _repositoryMock.Verify(
            repository => repository.GetAllUsersAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_RepositoryReturnsNoUsers_ReturnsEmptyCollection()
    {
        _repositoryMock
            .Setup(repository => repository.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetAllUsersAsync();

        result.Should().BeEmpty();
        _repositoryMock.Verify(
            repository => repository.GetAllUsersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_MapsEntityToAdminUserDto()
    {
        var user = new User
        {
            Id = 7,
            Name = "Sara Khan",
            Email = "sara@example.com",
            Role = Roles.Admin,
            PasswordHash = "must-not-be-returned"
        };
        _repositoryMock
            .Setup(repository => repository.GetUserByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserByIdAsync(7);

        result.Should().BeEquivalentTo(new AdminUserDto
        {
            Id = 7,
            Name = "Sara Khan",
            Email = "sara@example.com",
            Role = Roles.Admin,
            BookingCount = 0
        });
        _repositoryMock.Verify(
            repository => repository.GetUserByIdAsync(7, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.GetUserByIdAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserByIdAsync(404);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.User)]
    public async Task UpdateUserRoleAsync_KnownRole_UpdatesRepositoryAndReturnsTrue(string role)
    {
        _repositoryMock
            .Setup(repository => repository.UpdateUserRoleAsync(3, role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateUserRoleAsync(3, role);

        result.Should().BeTrue();
        _repositoryMock.Verify(
            repository => repository.UpdateUserRoleAsync(3, role, It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task UpdateUserRoleAsync_UserDoesNotExist_ReturnsFalse()
    {
        _repositoryMock
            .Setup(repository => repository.UpdateUserRoleAsync(404, Roles.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UpdateUserRoleAsync(404, Roles.Admin);

        result.Should().BeFalse();
        _loggerMock.VerifyLog(LogLevel.Information, Times.Never());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Guest")]
    [InlineData("admin")]
    public async Task UpdateUserRoleAsync_InvalidRole_ThrowsAndDoesNotCallRepository(string role)
    {
        Func<Task> action = () => _sut.UpdateUserRoleAsync(3, role);

        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage($"Invalid role '{role}'.*");
        _repositoryMock.Verify(
            repository => repository.UpdateUserRoleAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllBookingsAsync_RepositoryReturnsBookings_ReturnsThoseDtos()
    {
        var bookedAt = new DateTime(2026, 7, 29, 9, 30, 0, DateTimeKind.Utc);
        var bookings = new List<AdminBookingDto>
        {
            new()
            {
                Id = 11,
                UserId = 2,
                UserName = "Bilal",
                UserEmail = "bilal@example.com",
                EventId = 5,
                EventTitle = "Tech Meetup",
                Status = BookingStatus.Confirmed,
                BookedAt = bookedAt
            }
        };
        _repositoryMock
            .Setup(repository => repository.GetAllBookingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookings);

        var result = await _sut.GetAllBookingsAsync();

        result.Should().BeSameAs(bookings);
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(bookings[0]);
    }

    [Fact]
    public async Task GetAllBookingsAsync_RepositoryReturnsNoBookings_ReturnsEmptyCollection()
    {
        _repositoryMock
            .Setup(repository => repository.GetAllBookingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetAllBookingsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummaryAsync_RepositoryReturnsAnalytics_ReturnsMappedReport()
    {
        var summary = new AdminSummaryDto
        {
            TotalEvents = 4,
            TotalUsers = 9,
            TotalBookings = 7,
            ConfirmedBookings = 5,
            PendingBookings = 1,
            CancelledBookings = 1,
            BookingsPerDay =
            [
                new BookingsPerDayDto { Date = new DateOnly(2026, 7, 29), BookingCount = 2 }
            ],
            BookingsPerMonth =
            [
                new BookingsPerMonthDto { Year = 2026, Month = 7, BookingCount = 7 }
            ],
            BookingsPerEvent =
            [
                new BookingsPerEventDto { EventId = 5, Title = "Tech Meetup", BookingCount = 3 }
            ]
        };
        _repositoryMock
            .Setup(repository => repository.GetSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _sut.GetSummaryAsync();

        result.Should().BeSameAs(summary);
        result.Should().BeEquivalentTo(summary);
        _repositoryMock.Verify(
            repository => repository.GetSummaryAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSummaryAsync_RepositoryReturnsEmptyAnalytics_ReturnsZeroedReport()
    {
        var emptySummary = new AdminSummaryDto();
        _repositoryMock
            .Setup(repository => repository.GetSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptySummary);

        var result = await _sut.GetSummaryAsync();

        result.Should().BeEquivalentTo(new AdminSummaryDto());
    }
}

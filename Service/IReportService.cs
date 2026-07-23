using EventApi.Models;

namespace EventApi.Services;

public interface IReportService
{
    Task<IReadOnlyList<BookingsPerEventDto>> GetBookingsPerEventAsync(
        BookingStatus? status = null,
        CancellationToken cancellationToken = default);
}

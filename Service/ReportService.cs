using EventApi.Models;
using EventApi.Repositories;

namespace EventApi.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<IReadOnlyList<BookingsPerEventDto>> GetBookingsPerEventAsync(
        BookingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return _reportRepository.GetBookingsPerEventAsync(status, cancellationToken);
    }
}

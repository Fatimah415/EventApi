using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Bookings-per-event report (raw ADO.NET). Optional status filter, e.g.
    /// GET /api/reports/bookings-per-event?status=Confirmed
    /// </summary>
    [HttpGet("bookings-per-event")]
    public async Task<IActionResult> GetBookingsPerEvent(
        [FromQuery] BookingStatus? status,
        CancellationToken cancellationToken)
    {
        var report = await _reportService.GetBookingsPerEventAsync(status, cancellationToken);
        return Ok(report);
    }
}

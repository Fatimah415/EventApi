using EventApi.Models;
using EventApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reportRepository;

    public ReportsController(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    /// <summary>
    /// Bookings-per-event report (raw ADO.NET). Optional status filter, e.g.
    /// GET /api/reports/bookings-per-event?status=Confirmed
    /// </summary>
    [HttpGet("bookings-per-event")]
    public async Task<IActionResult> GetBookingsPerEvent([FromQuery] BookingStatus? status = null)
    {
        var report = await _reportRepository.GetBookingsPerEventAsync(status);
        return Ok(report);
    }
}

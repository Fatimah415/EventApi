using EventApi.Models;
using Microsoft.Data.SqlClient;

namespace EventApi.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<BookingsPerEventDto>> GetBookingsPerEventAsync(
        BookingStatus? status = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Raw ADO.NET reporting. This deliberately bypasses EF Core (see the class
/// remarks on <see cref="GetBookingsPerEventAsync"/>) and uses a hand-written,
/// fully parameterized SQL aggregate.
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly string _connectionString;

    public ReportRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    /// <summary>
    /// Bookings-per-event report. LEFT JOIN so events with zero bookings still
    /// appear; GROUP BY to aggregate the count. Optional status filter is passed
    /// as a SqlParameter — never string-concatenated — so the query is safe from
    /// SQL injection.
    /// </summary>
    public async Task<IReadOnlyList<BookingsPerEventDto>> GetBookingsPerEventAsync(
        BookingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT e.Id AS EventId,
                   e.Title AS Title,
                   COUNT(b.Id) AS BookingCount
            FROM Events e
            LEFT JOIN EventBookings b
                   ON b.EventId = e.Id
                  AND (@Status IS NULL OR b.Status = @Status)
            GROUP BY e.Id, e.Title
            ORDER BY BookingCount DESC, e.Id;";

        var results = new List<BookingsPerEventDto>();

        // 'using' guarantees the connection, command and reader are disposed even
        // if an exception is thrown mid-read.
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);

        // BookingStatus is stored as a string in the DB (HasConversion<string>),
        // so we pass the enum's name, or DBNull when no filter is requested.
        command.Parameters.Add(new SqlParameter("@Status", System.Data.SqlDbType.NVarChar, 20)
        {
            Value = status.HasValue ? status.Value.ToString() : DBNull.Value
        });

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BookingsPerEventDto
            {
                EventId = reader.GetInt32(reader.GetOrdinal("EventId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                BookingCount = reader.GetInt32(reader.GetOrdinal("BookingCount"))
            });
        }

        return results;
    }
}

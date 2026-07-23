using EventApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApi.Data.Configurations;

public class EventBookingConfiguration : IEntityTypeConfiguration<EventBooking>
{
    public void Configure(EntityTypeBuilder<EventBooking> builder)
    {
        builder.HasKey(b => b.Id);

        // Store the BookingStatus enum as a readable string ("Pending", ...)
        // instead of an int, so the column is self-explanatory and safe against
        // reordering the enum members.
        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.BookedAt)
            .IsRequired();

        // Event (1) ---- (*) EventBooking. Cascade: a booking cannot outlive its
        // event.
        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // User (1) ---- (*) EventBooking. Restrict avoids a second cascade path
        // to bookings (Event already cascades), which SQL Server forbids.
        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Required index.
        builder.HasIndex(b => b.EventId);
        // Useful extra: quickly list a user's bookings.
        builder.HasIndex(b => b.UserId);
        // The reporting endpoint filters bookings by status.
        builder.HasIndex(b => b.Status);

        // Seed bookings. Fixed ids/dates; every UserId and EventId is seeded.
        builder.HasData(
            new EventBooking { Id = 1, UserId = 2, EventId = 1, Status = BookingStatus.Confirmed, BookedAt = new DateTime(2026, 7, 6, 10, 0, 0) },
            new EventBooking { Id = 2, UserId = 2, EventId = 3, Status = BookingStatus.Pending,   BookedAt = new DateTime(2026, 7, 6, 11, 0, 0) },
            new EventBooking { Id = 3, UserId = 3, EventId = 1, Status = BookingStatus.Confirmed, BookedAt = new DateTime(2026, 7, 7, 9, 0, 0) },
            new EventBooking { Id = 4, UserId = 3, EventId = 5, Status = BookingStatus.Cancelled, BookedAt = new DateTime(2026, 7, 7, 12, 0, 0) },
            new EventBooking { Id = 5, UserId = 1, EventId = 9, Status = BookingStatus.Confirmed, BookedAt = new DateTime(2026, 7, 8, 14, 0, 0) });
    }
}

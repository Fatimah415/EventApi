using EventApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApi.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(200);

        // Event (*) ---- (1) User (owner). Cascade preserves the existing
        // behavior: deleting a user removes the events they own.
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Required indexes.
        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => e.CategoryId);

        // Seed 10 events. Fixed dates/ids keep HasData deterministic. Every
        // CategoryId (1-5) and UserId (1-3) points to a seeded row.
        builder.HasData(
            new Event { Id = 1,  Title = ".NET Conf 2026",        Description = "Annual .NET developer conference.", EventDate = new DateTime(2026, 9, 10, 9, 0, 0),  Location = "Lahore",    CategoryId = 1, UserId = 1, CreatedAt = new DateTime(2026, 7, 1, 12, 0, 0) },
            new Event { Id = 2,  Title = "EF Core Deep Dive",      Description = "Hands-on EF Core workshop.",         EventDate = new DateTime(2026, 9, 15, 10, 0, 0), Location = "Karachi",   CategoryId = 2, UserId = 1, CreatedAt = new DateTime(2026, 7, 1, 12, 0, 0) },
            new Event { Id = 3,  Title = "Coldplay Live",          Description = "Live in concert.",                   EventDate = new DateTime(2026, 10, 5, 19, 0, 0), Location = "Islamabad", CategoryId = 3, UserId = 2, CreatedAt = new DateTime(2026, 7, 2, 12, 0, 0) },
            new Event { Id = 4,  Title = "City Marathon",          Description = "Annual city marathon.",              EventDate = new DateTime(2026, 11, 1, 6, 30, 0), Location = "Lahore",    CategoryId = 4, UserId = 2, CreatedAt = new DateTime(2026, 7, 2, 12, 0, 0) },
            new Event { Id = 5,  Title = "Azure Meetup",           Description = "Cloud community meetup.",             EventDate = new DateTime(2026, 8, 20, 18, 0, 0), Location = "Karachi",   CategoryId = 5, UserId = 3, CreatedAt = new DateTime(2026, 7, 3, 12, 0, 0) },
            new Event { Id = 6,  Title = "AI Summit",              Description = "Artificial intelligence summit.",    EventDate = new DateTime(2026, 9, 25, 9, 0, 0),  Location = "Lahore",    CategoryId = 1, UserId = 1, CreatedAt = new DateTime(2026, 7, 3, 12, 0, 0) },
            new Event { Id = 7,  Title = "React Workshop",         Description = "Frontend workshop.",                 EventDate = new DateTime(2026, 10, 12, 10, 0, 0),Location = "Islamabad", CategoryId = 2, UserId = 3, CreatedAt = new DateTime(2026, 7, 4, 12, 0, 0) },
            new Event { Id = 8,  Title = "Jazz Night",             Description = "An evening of jazz.",                EventDate = new DateTime(2026, 11, 8, 20, 0, 0), Location = "Lahore",    CategoryId = 3, UserId = 2, CreatedAt = new DateTime(2026, 7, 4, 12, 0, 0) },
            new Event { Id = 9,  Title = "Cricket Finals",         Description = "National cricket finals.",           EventDate = new DateTime(2026, 12, 1, 15, 0, 0), Location = "Karachi",   CategoryId = 4, UserId = 1, CreatedAt = new DateTime(2026, 7, 5, 12, 0, 0) },
            new Event { Id = 10, Title = "Startup Meetup",         Description = "Founders networking meetup.",        EventDate = new DateTime(2026, 8, 30, 17, 0, 0), Location = "Islamabad", CategoryId = 5, UserId = 3, CreatedAt = new DateTime(2026, 7, 5, 12, 0, 0) });
    }
}

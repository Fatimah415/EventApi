using EventApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApi.Data.Configurations;

public class EventFavoriteConfiguration : IEntityTypeConfiguration<EventFavorite>
{
    public void Configure(EntityTypeBuilder<EventFavorite> builder)
    {
        // Composite primary key (UserId, EventId): the identity of a favorite is
        // the pairing itself, which also blocks duplicate favorites.
        builder.HasKey(f => new { f.UserId, f.EventId });

        builder.Property(f => f.FavoritedAt)
            .IsRequired();

        // Event (1) ---- (*) EventFavorite. Cascade: a favorite cannot outlive
        // its event.
        builder.HasOne(f => f.Event)
            .WithMany(e => e.Favorites)
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // User (1) ---- (*) EventFavorite. Restrict avoids a second cascade path
        // to favorites (Event already cascades), which SQL Server forbids.
        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Required index (the composite PK already indexes UserId first, but an
        // explicit index keeps the intent clear and matches the spec).
        builder.HasIndex(f => f.UserId);

        // Seed favorites. The composite key is (UserId, EventId), so each pair
        // is unique. Every UserId and EventId is a seeded row.
        builder.HasData(
            new EventFavorite { UserId = 2, EventId = 1, FavoritedAt = new DateTime(2026, 7, 6, 10, 30, 0) },
            new EventFavorite { UserId = 2, EventId = 6, FavoritedAt = new DateTime(2026, 7, 6, 10, 35, 0) },
            new EventFavorite { UserId = 3, EventId = 3, FavoritedAt = new DateTime(2026, 7, 7, 9, 15, 0) },
            new EventFavorite { UserId = 3, EventId = 7, FavoritedAt = new DateTime(2026, 7, 7, 9, 20, 0) },
            new EventFavorite { UserId = 1, EventId = 9, FavoritedAt = new DateTime(2026, 7, 8, 14, 30, 0) });
    }
}

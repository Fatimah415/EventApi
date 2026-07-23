using EventApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApi.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        // Category names must be unique.
        builder.HasIndex(c => c.Name).IsUnique();

        // Category (1) ---- (*) Events. Restrict: deleting a category must not
        // silently delete its events.
        builder.HasMany(c => c.Events)
            .WithOne(e => e.Category!)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed 5 categories with fixed ids (referenced by seeded events).
        builder.HasData(
            new Category { Id = 1, Name = "Conference", Description = "Large industry conferences and summits." },
            new Category { Id = 2, Name = "Workshop", Description = "Hands-on training workshops." },
            new Category { Id = 3, Name = "Concert", Description = "Live music and performances." },
            new Category { Id = 4, Name = "Sports", Description = "Sporting events and tournaments." },
            new Category { Id = 5, Name = "Meetup", Description = "Community and networking meetups." });
    }
}

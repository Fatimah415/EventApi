using System.Reflection;
using EventApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<EventBooking> EventBookings => Set<EventBooking>();

    public DbSet<EventFavorite> EventFavorites => Set<EventFavorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply every IEntityTypeConfiguration in this assembly
        // (Data/Configurations/*). This replaces the previous inline rules for
        // User (unique email) and Event (owner FK), which now live in
        // UserConfiguration and EventConfiguration.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

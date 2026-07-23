using EventApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);

        // Emails must be unique (moved here from OnModelCreating).
        builder.HasIndex(u => u.Email).IsUnique();

        // Seed users. Password hashes are pre-computed BCrypt (work factor 11)
        // constants — HasData must be deterministic, so we cannot hash at
        // runtime. Register and promote a local account for authentication demos.
        builder.HasData(
            new User
            {
                Id = 1,
                Name = "Admin User",
                Email = "admin@eventboard.com",
                PasswordHash = "$2a$11$ExjLvV2ie52b756HbndsOOLDoKEexPRtc9Sz7oxFigjZSGtiAaMA2",
                Role = Roles.Admin
            },
            new User
            {
                Id = 2,
                Name = "Sara Khan",
                Email = "sara@eventboard.com",
                PasswordHash = "$2a$11$B6Ctfp3GjbtOj8ZfvmiLtuQwnBvys/603hpg4OA36745T.AYNx.Qa",
                Role = Roles.User
            },
            new User
            {
                Id = 3,
                Name = "Bilal Ahmed",
                Email = "bilal@eventboard.com",
                PasswordHash = "$2a$11$mWlv.Rm30KrmZJR88vNNiew809Api5AXDNWBgRlooYEL6nnASB1m.",
                Role = Roles.User
            });
    }
}

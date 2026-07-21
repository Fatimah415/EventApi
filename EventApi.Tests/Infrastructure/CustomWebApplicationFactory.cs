using System;
using System.Linq;
using EventApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace EventApi.Tests.Infrastructure;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "EventApi.IntegrationTests.SigningKey.MustBeAtLeast32Bytes!");
        builder.UseSetting("Jwt:Issuer", "EventApi.Tests");
        builder.UseSetting("Jwt:Audience", "EventApi.Tests.Client");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            // Add an InMemory database with a unique name for this factory instance
            var dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });
            
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database
            // context (AppDbContext).
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();

                // Ensure the database is created.
                db.Database.EnsureCreated();
                
                // AppDbContext HasData records are inserted by EnsureCreated.
            }
        });
    }
}

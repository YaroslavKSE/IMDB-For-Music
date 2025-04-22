using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MusicLists.Infrastructure.DBConfig;
using System.IO;

namespace MusicLists.Infrastructure.DBConfig
{
    public class MusicListsDbContextFactory : IDesignTimeDbContextFactory<MusicListsDbContext>
    {
        public MusicListsDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            // Create the DbContextOptionsBuilder
            var optionsBuilder = new DbContextOptionsBuilder<MusicListsDbContext>();

            // Get connection string from configuration
            string connectionString = configuration.GetConnectionString("PostgreSQL") ??
                                      "Host=localhost;Database=MusicInteraction;Username=qualiaaa;Password=password";

            optionsBuilder.UseNpgsql(connectionString);

            // Create and return the DbContext instance using the options builder
            return new MusicListsDbContext(optionsBuilder.Options);
        }
    }
}
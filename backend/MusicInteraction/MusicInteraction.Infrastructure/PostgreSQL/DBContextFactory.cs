using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MusicInteraction.Infrastructure.PostgreSQL
{
    public class MusicInteractionDbContextFactory : IDesignTimeDbContextFactory<MusicInteractionDbContext>
    {
        public MusicInteractionDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("PostgreSQL") ??
                                   "Host=localhost;Database=MusicInteraction;Username=qualiaaa;Password=password";

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<MusicInteractionDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new MusicInteractionDbContext(configuration, optionsBuilder.Options);
        }
    }
}
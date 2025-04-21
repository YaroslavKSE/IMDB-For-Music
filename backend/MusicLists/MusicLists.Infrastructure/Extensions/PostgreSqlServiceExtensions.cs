using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MusicLists.Infrastructure.Extensions
{
    public static class PostgreSqlServiceExtensions
    {
        public static IServiceCollection AddPostgreSQLServices(this IServiceCollection services)
        {
            // Access the service provider
            var serviceProvider = services.BuildServiceProvider();
            
            // Get configuration
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            
            // // Register DB Context
            // services.AddDbContext<MusicListsDbContext>(options =>
            //     options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
            //
            // // Register repositories
            // services.AddScoped<IMusicListRepository, MusicListRepository>();

            return services;
        }
    }
}
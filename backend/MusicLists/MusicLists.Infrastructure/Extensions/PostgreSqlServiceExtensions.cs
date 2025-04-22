using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicInteraction.Infrastructure.Services;
using MusicLists.Application;
using MusicLists.Infrastructure.DBConfig;

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
            
            // Register DB Context
            services.AddDbContext<MusicListsDbContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("PostgreSQL");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("MusicLists.Infrastructure");
                });
            });

            services.BuildServiceProvider().GetRequiredService<MusicListsDbContext>().Database.Migrate();
            //
            // // Register repositories
            services.AddScoped<IMusicListsStorage, MusicListsStorage>();

            // Register the HotScore calculator
            services.AddSingleton<ListHotScoreCalculator>();

            // Register the background services
            services.AddHostedService<ListHotScoreUpdateService>();

            return services;
        }
    }
}
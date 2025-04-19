using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Infrastructure.Services;

namespace MusicInteraction.Infrastructure.PostgreSQL
{
    public static class PostgreSQLDependencyInjection
    {
        public static IServiceCollection AddPostgreSQLServices(this IServiceCollection services)
        {
            // Register the DbContext
            services.AddDbContext<MusicInteractionDbContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("PostgreSQL");

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("MusicInteraction.Infrastructure");
                });
            });

            services.BuildServiceProvider().GetRequiredService<MusicInteractionDbContext>().Database.Migrate();

            // Register the PostgreSQL implementation of IInteractionStorage
            services.AddScoped<IInteractionStorage, PostgreSQLInteractionStorage>();

            // Register the PostgreSQL implementation of IItemStatsStorage
            services.AddScoped<IItemStatsStorage, PostgreSQLItemStatsStorage>();

            // Register the background service
            services.AddHostedService<ItemStatsUpdateService>();

            return services;
        }
    }
}
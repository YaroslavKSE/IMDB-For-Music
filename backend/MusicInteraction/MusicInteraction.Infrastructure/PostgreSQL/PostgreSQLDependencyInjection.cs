using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicInteraction.Application.Interfaces;

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
                var connectionString = configuration.GetConnectionString("PostgreSQL") ??
                                       "Host=localhost;Database=MusicInteraction;Username=qualiaaa;Password=password";

                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("MusicInteraction.Infrastructure");
                });
            });

            services.BuildServiceProvider().GetRequiredService<MusicInteractionDbContext>().Database.Migrate();

            // Register the PostgreSQL implementation of IInteractionStorage
            services.AddScoped<IInteractionStorage, PostgreSQLInteractionStorage>();


            return services;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Infrastructure.PostgreSQL
{
    /// <summary>
    /// A simplified version of PostgreSQLMigrator that doesn't rely on migrations
    /// </summary>
    public class PostgreSQLDatabaseInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PostgreSQLDatabaseInitializer> _logger;

        public PostgreSQLDatabaseInitializer(IServiceProvider serviceProvider, ILogger<PostgreSQLDatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PostgreSQL database initialization started");

            using var scope = _serviceProvider.CreateScope();

            try
            {
                // Get the DbContext
                var dbContext = scope.ServiceProvider.GetRequiredService<MusicInteractionDbContext>();

                // Just try to ensure the database exists
                if (!await dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    _logger.LogInformation("Creating PostgreSQL database");
                    await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                    _logger.LogInformation("PostgreSQL database created successfully");
                }
                else
                {
                    _logger.LogInformation("PostgreSQL database already exists");
                }

                // No need to migrate schema - EnsureCreated will create tables based on the entity model

                // Check if data migration from local storage to PostgreSQL is needed
                await MigrateDataFromLocalStorage(scope, dbContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PostgreSQL database initialization");
                // Log the error but don't throw - we want the app to continue even if DB init fails
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task MigrateDataFromLocalStorage(IServiceScope scope, MusicInteractionDbContext dbContext, CancellationToken cancellationToken)
        {
            try
            {
                // Check if PostgreSQL database already has data
                if (await dbContext.Interactions.AnyAsync(cancellationToken))
                {
                    _logger.LogInformation("PostgreSQL database already contains data, skipping migration from local storage");
                    return;
                }

                // Try to get the local storage implementation
                var localStorageProvider = scope.ServiceProvider.GetService<LocalDBTemplate>();
                if (localStorageProvider == null)
                {
                    _logger.LogInformation("Local storage not available, skipping data migration");
                    return;
                }

                // Get all interactions from local storage
                var localInteractions = localStorageProvider.GetInteractions();
                if (localInteractions.Count == 0)
                {
                    _logger.LogInformation("No interactions found in local storage to migrate");
                    return;
                }

                _logger.LogInformation($"Found {localInteractions.Count} interactions in local storage to migrate");

                // Get the PostgreSQL storage implementation
                var postgresStorage = scope.ServiceProvider.GetRequiredService<IInteractionStorage>();

                // Migrate each interaction
                foreach (var interaction in localInteractions)
                {
                    try {
                        await postgresStorage.AddInteractionAsync(interaction);
                        _logger.LogInformation($"Migrated interaction {interaction.AggregateId} to PostgreSQL");
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, $"Failed to migrate interaction {interaction.AggregateId}");
                    }
                }

                _logger.LogInformation("Successfully migrated all interactions from local storage to PostgreSQL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating data from local storage to PostgreSQL");
            }
        }
    }

    // Extension method to register the database initializer
    public static class PostgreSQLDatabaseInitializerExtensions
    {
        public static IServiceCollection AddPostgreSQLDatabaseInitializer(this IServiceCollection services)
        {
            services.AddHostedService<PostgreSQLDatabaseInitializer>();
            return services;
        }
    }
}
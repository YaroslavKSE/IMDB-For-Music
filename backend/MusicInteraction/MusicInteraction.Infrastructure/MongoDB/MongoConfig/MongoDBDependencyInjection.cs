using Microsoft.Extensions.DependencyInjection;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Infrastructure.MongoDB;

namespace MusicInteraction.Infrastructure.MongoDB;

public static class MongoDbDependencyInjection
{
    public static IServiceCollection AddMongoDbServices(this IServiceCollection services)
    {
        // Register MongoDB context
        services.AddSingleton<MongoDbContext>();

        // Register MongoDB repositories
        services.AddSingleton<IGradingMethodRepository, GradingMethodRepository>();

        // Replace LocalStorage implementation with MongoDB implementation
        services.AddSingleton<IGradingMethodStorage, MongoGradingMethodStorage>();

        return services;
    }
}
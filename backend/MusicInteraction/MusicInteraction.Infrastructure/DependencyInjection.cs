using Microsoft.Extensions.DependencyInjection;
using MusicInteraction.Application;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Infrastructure.LocalStorages;

namespace MusicInteraction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IInteractionStorage, InteractionStorage>();
        services.AddSingleton<IGradingMethodStorage, GradingMethodStorage>();
        return services;
    }
}
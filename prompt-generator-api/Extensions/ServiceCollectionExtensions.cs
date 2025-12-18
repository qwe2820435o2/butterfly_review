// Extensions/ServiceCollectionExtensions.cs

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace tennis_wave_api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register all Services
        var serviceTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Service"));

        foreach (var impl in serviceTypes)
        {
            var iface = impl.GetInterface($"I{impl.Name}");
            if (iface != null)
            {
                services.AddScoped(iface, impl);
            }
        }

        // Register all Repositories
        var repositoryTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Repository"));

        foreach (var impl in repositoryTypes)
        {
            var iface = impl.GetInterface($"I{impl.Name}");
            if (iface != null)
            {
                services.AddScoped(iface, impl);
            }
        }

        return services;
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Dependency-injection registration for FancyBlazor.</summary>
public static class FancyBlazorServiceCollectionExtensions
{
    /// <summary>Registers the shared FancyBlazor runtime and optional defaults.</summary>
    public static IServiceCollection AddFancyBlazor(
        this IServiceCollection services,
        Action<FancyBlazorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<FancyBlazorOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.TryAddScoped<IFancyEffectRuntime, FancyEffectRuntime>();
        return services;
    }
}

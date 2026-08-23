using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Dependency-injection registration for optional FancyBlazor WebGL effects.</summary>
public static class FancyBlazorWebGlServiceCollectionExtensions
{
    /// <summary>Registers the companion WebGL runtime and optional resource limits.</summary>
    public static IServiceCollection AddFancyBlazorWebGl(
        this IServiceCollection services,
        Action<FancyWebGlOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddFancyBlazor();
        var optionsBuilder = services.AddOptions<FancyWebGlOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.TryAddScoped<IFancyWebGlRuntime, FancyWebGlRuntime>();
        return services;
    }
}

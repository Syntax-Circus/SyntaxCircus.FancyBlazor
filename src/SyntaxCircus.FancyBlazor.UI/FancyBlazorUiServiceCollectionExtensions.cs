using Microsoft.Extensions.DependencyInjection;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Dependency-injection registration for the optional FancyBlazor UI companion.</summary>
public static class FancyBlazorUiServiceCollectionExtensions
{
    /// <summary>Registers the UI companion's typed theme defaults.</summary>
    public static IServiceCollection AddFancyBlazorUi(
        this IServiceCollection services,
        Action<FancyUiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddFancyBlazor();
        var optionsBuilder = services.AddOptions<FancyUiOptions>();
        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyUiOptionsTests
{
    [Fact]
    public void AddFancyBlazorUi_WithoutConfiguration_UsesDefaultTheme()
    {
        var services = new ServiceCollection();

        var returnedServices = services.AddFancyBlazorUi();

        returnedServices.ShouldBeSameAs(services);
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<FancyUiOptions>>().Value.Theme.ShouldBeSameAs(FancyUiThemes.Default);
    }

    [Fact]
    public void AddFancyBlazorUi_WithConfiguration_OverridesTheme()
    {
        var services = new ServiceCollection();
        var customTheme = new FancyUiTheme("#fff", "#000", "#ccc", "#f00", "1rem", "1rem", "#0f0");

        services.AddFancyBlazorUi(options => options.Theme = customTheme);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<FancyUiOptions>>().Value.Theme.ShouldBeSameAs(customTheme);
    }

    [Fact]
    public void AddFancyBlazorUi_ChainsCoreRegistration()
    {
        var services = new ServiceCollection();

        services.AddFancyBlazorUi();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<FancyBlazorOptions>>().Value.MotionPreference.ShouldBe(FancyMotionPreference.RespectSystem);
    }

    [Fact]
    public void AddFancyBlazorUi_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Should.Throw<ArgumentNullException>(() => services.AddFancyBlazorUi());
    }
}

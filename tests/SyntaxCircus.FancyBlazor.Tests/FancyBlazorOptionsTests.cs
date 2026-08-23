using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.Tests;

public sealed class FancyBlazorOptionsTests
{
    [Fact]
    public void AddFancyBlazor_WithoutConfiguration_RegistersAccessibleDefaults()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFancyBlazor();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<FancyBlazorOptions>>().Value;

        options.MotionPreference.ShouldBe(FancyMotionPreference.RespectSystem);
        options.Quality.ShouldBe(FancyQuality.Auto);
        options.PauseWhenHidden.ShouldBeTrue();
        options.PauseWhenOffscreen.ShouldBeTrue();
        options.EnableDiagnostics.ShouldBeFalse();
    }

    [Fact]
    public void AddFancyBlazor_WithConfiguration_AppliesOverrides()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFancyBlazor(options =>
        {
            options.MotionPreference = FancyMotionPreference.AlwaysReduce;
            options.Quality = FancyQuality.Low;
            options.PauseWhenHidden = false;
            options.EnableDiagnostics = true;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<FancyBlazorOptions>>().Value;

        options.MotionPreference.ShouldBe(FancyMotionPreference.AlwaysReduce);
        options.Quality.ShouldBe(FancyQuality.Low);
        options.PauseWhenHidden.ShouldBeFalse();
        options.EnableDiagnostics.ShouldBeTrue();
    }

    [Fact]
    public void FancyPalette_WithBlankColor_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => new FancyPalette(" ", "#fff", "#fff", "#000"));
    }

    [Fact]
    public void FancyPalettes_ExposeFourNonEmptyCssColors()
    {
        var palette = FancyPalettes.Witchlight;

        palette.Primary.ShouldNotBeNullOrWhiteSpace();
        palette.Secondary.ShouldNotBeNullOrWhiteSpace();
        palette.Accent.ShouldNotBeNullOrWhiteSpace();
        palette.Background.ShouldNotBeNullOrWhiteSpace();
    }
}

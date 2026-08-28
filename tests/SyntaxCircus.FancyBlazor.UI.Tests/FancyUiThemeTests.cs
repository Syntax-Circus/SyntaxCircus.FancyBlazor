using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyUiThemeTests
{
    [Fact]
    public void Constructor_TrimsProvidedValues()
    {
        var theme = new FancyUiTheme(" #fff ", " #000 ", " #ccc ", " #f00 ", " 1rem ", " 1rem ", " #0f0 ");

        theme.Surface.ShouldBe("#fff");
        theme.Text.ShouldBe("#000");
        theme.Border.ShouldBe("#ccc");
        theme.Accent.ShouldBe("#f00");
        theme.Radius.ShouldBe("1rem");
        theme.Spacing.ShouldBe("1rem");
        theme.FocusRing.ShouldBe("#0f0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsMissingSurface(string? value)
    {
        Should.Throw<ArgumentException>(() => new FancyUiTheme(value!, "#000", "#ccc", "#f00", "1rem", "1rem", "#0f0"));
    }

    [Fact]
    public void Default_IsDerivedFromWitchlightPalette()
    {
        FancyUiThemes.Default.Accent.ShouldBe(FancyPalettes.Witchlight.Primary);
        FancyUiThemes.Default.FocusRing.ShouldBe(FancyPalettes.Witchlight.Accent);
    }
}

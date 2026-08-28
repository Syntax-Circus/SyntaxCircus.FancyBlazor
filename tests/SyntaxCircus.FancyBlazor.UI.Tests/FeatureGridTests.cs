using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FeatureGridTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FeatureGrid_RendersListWithStableHook()
    {
        using var context = CreateContext();

        var cut = context.Render<FeatureGrid>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Fast</li>")));

        var list = cut.Find("ul");
        list.ClassList.ShouldContain("syntax-circus-fancy-ui-feature-grid");
        cut.FindAll("li").Count.ShouldBe(1);
    }

    [Fact]
    public void FeatureGrid_RendersMultipleFeatureItems()
    {
        using var context = CreateContext();

        var cut = context.Render<FeatureGrid>(parameters => parameters
            .Add(component => component.ChildContent, builder =>
            {
                builder.AddMarkupContent(0, "<li>Fast</li><li>Accessible</li><li>Themeable</li>");
            }));

        cut.FindAll("li").Count.ShouldBe(3);
    }

    [Fact]
    public void FeatureGrid_DefaultColumnsIsThree()
    {
        using var context = CreateContext();

        var cut = context.Render<FeatureGrid>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Fast</li>")));

        cut.Find("ul").ClassList.ShouldContain("syntax-circus-fancy-ui-feature-grid--three");
    }

    [Theory]
    [InlineData(FeatureGridColumns.Two, "syntax-circus-fancy-ui-feature-grid--two")]
    [InlineData(FeatureGridColumns.Four, "syntax-circus-fancy-ui-feature-grid--four")]
    public void FeatureGrid_Columns_SetsExpectedModifierClass(FeatureGridColumns columns, string expectedClass)
    {
        using var context = CreateContext();

        var cut = context.Render<FeatureGrid>(parameters => parameters
            .Add(component => component.Columns, columns)
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Fast</li>")));

        cut.Find("ul").ClassList.ShouldContain(expectedClass);
    }

    [Fact]
    public void FeatureGrid_WithoutAriaLabel_OmitsAttribute()
    {
        using var context = CreateContext();

        var cut = context.Render<FeatureGrid>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Fast</li>")));

        cut.Find("ul").HasAttribute("aria-label").ShouldBeFalse();
    }

    [Fact]
    public void FeatureGrid_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<FeatureGrid>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Fast</li>"))
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "marketing-features")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var list = cut.Find("ul");
        list.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-feature-grid syntax-circus-fancy-ui-feature-grid--three marketing-features test-hook");
        (list.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-text:#eee");
    }
}

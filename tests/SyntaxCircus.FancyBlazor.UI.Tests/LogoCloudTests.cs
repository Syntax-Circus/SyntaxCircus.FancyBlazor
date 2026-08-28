using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class LogoCloudTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void LogoCloud_RendersListWithStableHook()
    {
        using var context = CreateContext();

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Acme</li>")));

        var list = cut.Find("ul");
        list.ClassList.ShouldContain("syntax-circus-fancy-ui-logo-cloud");
        cut.FindAll("li").Count.ShouldBe(1);
    }

    [Fact]
    public void LogoCloud_RendersConsumerSuppliedListItems()
    {
        using var context = CreateContext();

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.ChildContent, builder =>
            {
                builder.AddMarkupContent(0, "<li>Acme</li><li>Globex</li><li>Initech</li>");
            }));

        cut.FindAll("li").Count.ShouldBe(3);
    }

    [Fact]
    public void LogoCloud_WithoutAriaLabel_OmitsAttribute()
    {
        using var context = CreateContext();

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Acme</li>")));

        cut.Find("ul").HasAttribute("aria-label").ShouldBeFalse();
    }

    [Fact]
    public void LogoCloud_WithAriaLabel_SetsAttribute()
    {
        using var context = CreateContext();

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.AriaLabel, "Trusted by")
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Acme</li>")));

        cut.Find("ul").GetAttribute("aria-label").ShouldBe("Trusted by");
    }

    [Fact]
    public void LogoCloud_DefaultLayoutIsWrap()
    {
        using var context = CreateContext();

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Acme</li>")));

        cut.Find("ul").ClassList.ShouldNotContain("syntax-circus-fancy-ui-logo-cloud--dense");
    }

    [Fact]
    public void LogoCloud_DenseLayout_AddsModifierClass()
    {
        using var context = CreateContext();

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.Layout, LogoCloudLayout.Dense)
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Acme</li>")));

        var list = cut.Find("ul");
        list.ClassList.ShouldContain("syntax-circus-fancy-ui-logo-cloud");
        list.ClassList.ShouldContain("syntax-circus-fancy-ui-logo-cloud--dense");
    }

    [Fact]
    public void LogoCloud_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<LogoCloud>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<li>Acme</li>"))
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "marketing-logo-row")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var list = cut.Find("ul");
        list.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-logo-cloud marketing-logo-row test-hook");
        (list.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-text:#eee");
    }
}

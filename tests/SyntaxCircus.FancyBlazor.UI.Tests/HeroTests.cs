using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class HeroTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void Hero_WithoutSlots_RendersEmptyContent()
    {
        using var context = CreateContext();

        var cut = context.Render<Hero>();

        cut.Find("div").ClassList.ShouldContain("syntax-circus-fancy-ui-hero");
        cut.FindAll(".syntax-circus-fancy-ui-hero__heading").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-hero__subheading").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-hero__actions").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-hero__background").ShouldBeEmpty();
    }

    [Fact]
    public void Hero_WithAllSlots_RendersAllSlots()
    {
        using var context = CreateContext();

        var cut = context.Render<Hero>(parameters => parameters
            .Add(component => component.Heading, builder => builder.AddMarkupContent(0, "<h1>Ship faster</h1>"))
            .Add(component => component.Subheading, builder => builder.AddContent(0, "Composable effects for Blazor."))
            .Add(component => component.Actions, builder => builder.AddMarkupContent(0, "<a href=\"/start\">Get started</a>"))
            .Add(component => component.Background, builder => builder.AddMarkupContent(0, "<div class=\"gradient\"></div>")));

        cut.Find(".syntax-circus-fancy-ui-hero__heading").InnerHtml.ShouldContain("Ship faster");
        cut.Find(".syntax-circus-fancy-ui-hero__subheading").TextContent.ShouldBe("Composable effects for Blazor.");
        cut.Find(".syntax-circus-fancy-ui-hero__actions").QuerySelector("a")!.TextContent.ShouldBe("Get started");
        cut.Find(".syntax-circus-fancy-ui-hero__background").ShouldNotBeNull();
    }

    [Fact]
    public void Hero_Background_IsAriaHidden()
    {
        using var context = CreateContext();

        var cut = context.Render<Hero>(parameters => parameters
            .Add(component => component.Background, builder => builder.AddMarkupContent(0, "<div class=\"gradient\"></div>")));

        cut.Find(".syntax-circus-fancy-ui-hero__background").GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Hero_DefaultAlignmentIsStart()
    {
        using var context = CreateContext();

        var cut = context.Render<Hero>();

        cut.Find("div").ClassList.ShouldNotContain("syntax-circus-fancy-ui-hero--center");
    }

    [Fact]
    public void Hero_CenterAlignment_AddsModifierClass()
    {
        using var context = CreateContext();

        var cut = context.Render<Hero>(parameters => parameters
            .Add(component => component.Alignment, HeroAlignment.Center));

        var root = cut.Find("div");
        root.ClassList.ShouldContain("syntax-circus-fancy-ui-hero");
        root.ClassList.ShouldContain("syntax-circus-fancy-ui-hero--center");
    }

    [Fact]
    public void Hero_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<Hero>(parameters => parameters
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "landing-hero")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var root = cut.Find("div");
        root.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-hero landing-hero test-hook");
        (root.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-accent:#f00");
    }
}

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class CallToActionTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void CallToAction_WithoutSlots_RendersEmptyRoot()
    {
        using var context = CreateContext();

        var cut = context.Render<CallToAction>();

        cut.Find("div").ClassList.ShouldContain("syntax-circus-fancy-ui-cta");
        cut.FindAll(".syntax-circus-fancy-ui-cta__heading").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-cta__body").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-cta__actions").ShouldBeEmpty();
    }

    [Fact]
    public void CallToAction_WithAllSlots_RendersAllSlots()
    {
        using var context = CreateContext();

        var cut = context.Render<CallToAction>(parameters => parameters
            .Add(component => component.Heading, builder => builder.AddMarkupContent(0, "<h2>Ship faster</h2>"))
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Start building today."))
            .Add(component => component.Actions, builder => builder.AddMarkupContent(0, "<a href=\"/start\">Get started</a>")));

        cut.Find(".syntax-circus-fancy-ui-cta__heading").InnerHtml.ShouldContain("Ship faster");
        cut.Find(".syntax-circus-fancy-ui-cta__body").TextContent.ShouldBe("Start building today.");
        cut.Find(".syntax-circus-fancy-ui-cta__actions").QuerySelector("a")!.TextContent.ShouldBe("Get started");
    }

    [Fact]
    public void CallToAction_DefaultLayoutIsInline()
    {
        using var context = CreateContext();

        var cut = context.Render<CallToAction>();

        cut.Find("div").ClassList.ShouldNotContain("syntax-circus-fancy-ui-cta--stacked");
    }

    [Fact]
    public void CallToAction_StackedLayout_AddsModifierClass()
    {
        using var context = CreateContext();

        var cut = context.Render<CallToAction>(parameters => parameters
            .Add(component => component.Layout, CallToActionLayout.Stacked));

        var root = cut.Find("div");
        root.ClassList.ShouldContain("syntax-circus-fancy-ui-cta");
        root.ClassList.ShouldContain("syntax-circus-fancy-ui-cta--stacked");
    }

    [Fact]
    public void CallToAction_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<CallToAction>(parameters => parameters
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "footer-cta")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var root = cut.Find("div");
        root.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-cta footer-cta test-hook");
        (root.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-accent:#f00");
    }
}

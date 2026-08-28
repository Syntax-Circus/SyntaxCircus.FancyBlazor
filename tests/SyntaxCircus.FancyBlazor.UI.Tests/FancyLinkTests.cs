using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyLinkTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FancyLink_RendersHrefAndStableHook()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyLink>(parameters => parameters
            .Add(component => component.Href, "/details")
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Open details")));

        var anchor = cut.Find("a");
        anchor.GetAttribute("href").ShouldBe("/details");
        anchor.ClassList.ShouldContain("syntax-circus-fancy-ui-link");
        anchor.HasAttribute("aria-disabled").ShouldBeFalse();
    }

    [Fact]
    public void FancyLink_BlankTarget_AddsSafeRelByDefault()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyLink>(parameters => parameters
            .Add(component => component.Href, "https://example.com")
            .Add(component => component.Target, "_blank")
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "External")));

        var anchor = cut.Find("a");
        anchor.GetAttribute("target").ShouldBe("_blank");
        anchor.GetAttribute("rel").ShouldBe("noopener noreferrer");
    }

    [Fact]
    public void FancyLink_BlankTarget_RespectsExplicitRel()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyLink>(parameters => parameters
            .Add(component => component.Href, "https://example.com")
            .Add(component => component.Target, "_blank")
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "External"))
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["rel"] = "author" }));

        cut.Find("a").GetAttribute("rel").ShouldBe("author");
    }

    [Fact]
    public void FancyLink_Disabled_OmitsHrefAndSetsAriaDisabled()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyLink>(parameters => parameters
            .Add(component => component.Href, "/details")
            .Add(component => component.Disabled, true)
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Open details")));

        var anchor = cut.Find("a");
        anchor.HasAttribute("href").ShouldBeFalse();
        anchor.GetAttribute("aria-disabled").ShouldBe("true");
    }

    [Fact]
    public void FancyLink_MergesAttributesAndPreservesChildContent()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyLink>(parameters => parameters
            .Add(component => component.Href, "/details")
            .Add(component => component.CssClass, "nav-link")
            .Add(component => component.Style, "margin:0.5rem")
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, "<strong>Details</strong>"))
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["class"] = "test-hook",
                ["style"] = "padding:0.25rem",
            }));

        var anchor = cut.Find("a");
        anchor.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-link nav-link test-hook");
        var style = anchor.GetAttribute("style") ?? string.Empty;
        style.ShouldContain("margin:0.5rem");
        style.ShouldContain("padding:0.25rem");
        cut.Markup.ShouldContain("<strong>Details</strong>");
    }
}

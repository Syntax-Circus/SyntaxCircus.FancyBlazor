using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyNavbarTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FancyNavbar_WithoutSlots_RendersEmptyLandmarkWithDefaultLabel()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyNavbar>();

        var nav = cut.Find("nav");
        nav.GetAttribute("aria-label").ShouldBe("Primary");
        nav.ClassList.ShouldContain("syntax-circus-fancy-ui-navbar");
        cut.FindAll(".syntax-circus-fancy-ui-navbar__brand").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-navbar__links").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-navbar__actions").ShouldBeEmpty();
    }

    [Fact]
    public void FancyNavbar_WithAllSlots_RendersEachOnlyWhenProvided()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyNavbar>(parameters => parameters
            .Add(component => component.AriaLabel, "Site")
            .Add(component => component.Brand, builder => builder.AddMarkupContent(0, "<a href=\"/\">Acme</a>"))
            .Add(component => component.Links, builder => builder.AddMarkupContent(0, "<a href=\"/pricing\">Pricing</a>"))
            .Add(component => component.Actions, builder => builder.AddMarkupContent(0, "<button type=\"button\">Sign in</button>")));

        var nav = cut.Find("nav");
        nav.GetAttribute("aria-label").ShouldBe("Site");
        cut.Find(".syntax-circus-fancy-ui-navbar__brand").InnerHtml.ShouldContain("Acme");
        cut.Find(".syntax-circus-fancy-ui-navbar__links").InnerHtml.ShouldContain("Pricing");
        cut.Find(".syntax-circus-fancy-ui-navbar__actions").InnerHtml.ShouldContain("Sign in");
    }

    [Fact]
    public void FancyNavbar_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<FancyNavbar>(parameters => parameters
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "site-navbar")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var nav = cut.Find("nav");
        nav.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-navbar site-navbar test-hook");
        (nav.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-surface:#111");
    }
}

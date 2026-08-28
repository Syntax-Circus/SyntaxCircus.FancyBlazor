using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class TestimonialTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void Testimonial_WithoutAttribution_RendersOnlyQuote()
    {
        using var context = CreateContext();

        var cut = context.Render<Testimonial>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Best library I've used.")));

        cut.Find("figure").ClassList.ShouldContain("syntax-circus-fancy-ui-testimonial");
        cut.Find(".syntax-circus-fancy-ui-testimonial__quote").TextContent.ShouldBe("Best library I've used.");
        cut.FindAll("figcaption").ShouldBeEmpty();
    }

    [Fact]
    public void Testimonial_WithAttribution_RendersFigcaption()
    {
        using var context = CreateContext();

        var cut = context.Render<Testimonial>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Best library I've used."))
            .Add(component => component.Attribution, builder => builder.AddMarkupContent(0, "<cite>Jane Doe</cite>, Acme")));

        var caption = cut.Find(".syntax-circus-fancy-ui-testimonial__attribution");
        caption.QuerySelector("cite")!.TextContent.ShouldBe("Jane Doe");
        caption.TextContent.ShouldContain("Acme");
    }

    [Fact]
    public void Testimonial_WithoutAvatar_OmitsAvatarWrapper()
    {
        using var context = CreateContext();

        var cut = context.Render<Testimonial>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Quote"))
            .Add(component => component.Attribution, builder => builder.AddContent(0, "Jane Doe")));

        cut.FindAll(".syntax-circus-fancy-ui-testimonial__avatar").ShouldBeEmpty();
    }

    [Fact]
    public void Testimonial_WithAvatar_RendersAvatarBeforeAttributionContent()
    {
        using var context = CreateContext();

        var cut = context.Render<Testimonial>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Quote"))
            .Add(component => component.Attribution, builder => builder.AddContent(0, "Jane Doe"))
            .Add(component => component.Avatar, builder => builder.AddMarkupContent(0, "<img src=\"/avatar.png\" alt=\"\" />")));

        var caption = cut.Find(".syntax-circus-fancy-ui-testimonial__attribution");
        caption.QuerySelector(".syntax-circus-fancy-ui-testimonial__avatar img").ShouldNotBeNull();
        caption.TextContent.ShouldContain("Jane Doe");
    }

    [Fact]
    public void Testimonial_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<Testimonial>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Quote"))
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "hero-testimonial")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var figure = cut.Find("figure");
        figure.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-testimonial hero-testimonial test-hook");
        (figure.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-surface:#111");
    }
}

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.Tests;

public sealed class ComponentContractTests
{
    private const string ModulePath = "./_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js";

    [Fact]
    public void ShaderBackground_Defaults_PreserveSemanticContentAndDecorativeCanvas()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<h1>Semantic heading</h1>");

        var cut = context.Render<ShaderBackground>(parameters => parameters
            .Add(component => component.ChildContent, content)
            .AddUnmatched("data-testid", "shader"));

        cut.Markup.ShouldContain("syntax-circus-fancy-shader-background");
        cut.Markup.ShouldContain("data-testid=\"shader\"");
        cut.Markup.ShouldContain("<h1>Semantic heading</h1>");
        cut.Markup.ShouldContain("aria-hidden=\"true\"");
        cut.Markup.ShouldContain("tabindex=\"-1\"");
        cut.Markup.ShouldContain("--sc-fancy-background:#08111f");
    }

    [Fact]
    public void GlowBorder_CustomAttributes_MergeClassStyleAndClampValues()
    {
        using var context = new BunitContext();
        RenderFragment content = builder => builder.AddContent(0, "Card");

        var markup = context.Render<GlowBorder>(parameters => parameters
            .Add(component => component.ChildContent, content)
            .Add(component => component.CssClass, "product-glow")
            .Add(component => component.Intensity, 10)
            .Add(component => component.Radius, -4)
            .Add(component => component.Duration, TimeSpan.FromMilliseconds(725))
            .AddUnmatched("class", "test-hook")
            .AddUnmatched("style", "margin:1rem")
            .AddUnmatched("aria-label", "Featured card"))
            .Markup;

        markup.ShouldContain("syntax-circus-fancy-glow-border product-glow test-hook");
        markup.ShouldContain("--sc-fancy-glow-intensity:1");
        markup.ShouldContain("--sc-fancy-radius:0px");
        markup.ShouldContain("--sc-fancy-duration:725ms");
        markup.ShouldContain("margin:1rem;");
        markup.ShouldContain("aria-label=\"Featured card\"");
    }

    [Fact]
    public void Reveal_Defaults_RenderContentWithoutAriaHidingIt()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<p>Readable now</p>");

        var markup = context.Render<Reveal>(parameters => parameters
            .Add(component => component.ChildContent, content))
            .Markup;

        markup.ShouldContain("data-fancy-reveal=\"fadeup\"");
        markup.ShouldContain("--sc-fancy-duration:500ms");
        markup.ShouldContain("<p>Readable now</p>");
        markup.ShouldNotContain("aria-hidden");
    }

    [Fact]
    public void Tilt_WithGlare_PreservesInteractiveChildrenAndHidesOnlyGlare()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var markup = context.Render<Tilt>(parameters => parameters
            .Add(component => component.ChildContent, content)
            .Add(component => component.Glare, true))
            .Markup;

        markup.ShouldContain("syntax-circus-fancy-tilt__glare");
        markup.ShouldContain("aria-hidden=\"true\"");
        markup.ShouldContain("<a href=\"/next\">Continue</a>");
        markup.ShouldNotContain("tabindex");
        markup.ShouldNotContain("role=");
    }

    [Fact]
    public void Components_ComposeWithoutRemovingNestedContent()
    {
        using var context = CreateContext();
        RenderFragment label = builder => builder.AddMarkupContent(0, "<button type=\"button\">Launch</button>");
        RenderFragment glow = builder =>
        {
            builder.OpenComponent<GlowBorder>(0);
            builder.AddAttribute(1, nameof(GlowBorder.ChildContent), label);
            builder.CloseComponent();
        };
        RenderFragment tilt = builder =>
        {
            builder.OpenComponent<Tilt>(0);
            builder.AddAttribute(1, nameof(Tilt.ChildContent), glow);
            builder.CloseComponent();
        };

        var markup = context.Render<Reveal>(parameters => parameters
            .Add(component => component.ChildContent, tilt))
            .Markup;

        markup.ShouldContain("syntax-circus-fancy-reveal");
        markup.ShouldContain("syntax-circus-fancy-tilt");
        markup.ShouldContain("syntax-circus-fancy-glow-border");
        markup.ShouldContain("<button type=\"button\">Launch</button>");
    }

    [Fact]
    public void ExpandedCssEffects_RenderStableHooksAndClampValues()
    {
        using var context = new BunitContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<article>Readable</article>");

        var gradient = context.Render<GradientBackground>(p => p.Add(x => x.ChildContent, content).Add(x => x.Angle, 999)).Markup;
        var shimmer = context.Render<Shimmer>(p => p.Add(x => x.ChildContent, content).Add(x => x.Intensity, 3)).Markup;

        gradient.ShouldContain("syntax-circus-fancy-gradient-background");
        gradient.ShouldContain("--sc-fancy-gradient-angle:360deg");
        shimmer.ShouldContain("syntax-circus-fancy-shimmer__layer");
        shimmer.ShouldContain("aria-hidden=\"true\"");
        shimmer.ShouldContain("--sc-fancy-shimmer-intensity:1");
    }

    [Fact]
    public void ExpandedRuntimeEffects_PreserveSemanticContentAndAccessibility()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var spotlight = context.Render<Spotlight>(p => p.Add(x => x.ChildContent, content)).Markup;
        var magnetic = context.Render<Magnetic>(p => p.Add(x => x.ChildContent, content).Add(x => x.Strength, -1)).Markup;
        var parallax = context.Render<Parallax>(p => p.Add(x => x.ChildContent, content).Add(x => x.Distance, 999)).Markup;
        var stagger = context.Render<Stagger>(p => p.Add(x => x.ChildContent, content)).Markup;

        spotlight.ShouldContain("syntax-circus-fancy-spotlight__light");
        spotlight.ShouldContain("aria-hidden=\"true\"");
        magnetic.ShouldContain("--sc-fancy-magnetic-strength:0");
        parallax.ShouldContain("--sc-fancy-parallax-distance:300px");
        stagger.ShouldContain("data-fancy-effect=\"stagger\"");
        stagger.ShouldContain("<a href=\"/next\">Continue</a>");
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazor();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(1);
        module.SetupVoid("updateEffect", _ => true);
        module.SetupVoid("destroyEffect", _ => true);
        return context;
    }
}

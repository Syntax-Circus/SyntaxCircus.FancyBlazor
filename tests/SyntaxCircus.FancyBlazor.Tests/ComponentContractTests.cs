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

    [Fact]
    public void ExpressiveCssEffects_RenderStableHooksAndDecorativeLayers()
    {
        using var context = new BunitContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<article>Readable</article>");

        var gradient = context.Render<GradientText>(p => p.Add(x => x.ChildContent, content).Add(x => x.Angle, 999)).Markup;
        var aurora = context.Render<AuroraBackground>(p => p.Add(x => x.ChildContent, content).Add(x => x.Intensity, 3)).Markup;
        var noise = context.Render<NoiseOverlay>(p => p.Add(x => x.ChildContent, content).Add(x => x.Opacity, -1)).Markup;

        gradient.ShouldContain("--sc-fancy-gradient-angle:360deg");
        aurora.ShouldContain("syntax-circus-fancy-aurora-background__layer");
        aurora.ShouldContain("aria-hidden=\"true\"");
        aurora.ShouldContain("--sc-fancy-aurora-intensity:1");
        noise.ShouldContain("--sc-fancy-noise-opacity:0");
    }

    [Fact]
    public void ExpressiveRuntimeEffects_PreserveSemanticsAndClampValues()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var text = context.Render<TextReveal>(p => p.Add(x => x.Text, "Semantic heading").Add(x => x.Element, TextRevealElement.Heading2)).Markup;
        var ripple = context.Render<Ripple>(p => p.Add(x => x.ChildContent, content).Add(x => x.Opacity, 3)).Markup;
        var trail = context.Render<CursorTrail>(p => p.Add(x => x.ChildContent, content).Add(x => x.Size, 0).Add(x => x.ParticleCount, 99)).Markup;

        text.ShouldContain("<h2");
        text.ShouldContain("Semantic heading");
        ripple.ShouldContain("--sc-fancy-ripple-opacity:1");
        ripple.ShouldContain("<a href=\"/next\">Continue</a>");
        trail.ShouldContain("syntax-circus-fancy-cursor-trail__canvas");
        trail.ShouldContain("aria-hidden=\"true\"");
        trail.ShouldContain("--sc-fancy-cursor-trail-size:4px");
    }

    [Fact]
    public void SpatialSurfaceEffects_PreserveSemanticsAndClampValues()
    {
        using var context = new BunitContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var glass = context.Render<GlassSurface>(p => p.Add(x => x.ChildContent, content).Add(x => x.Blur, 99).Add(x => x.BorderOpacity, -1)).Markup;
        var beam = context.Render<BorderBeam>(p => p.Add(x => x.ChildContent, content).Add(x => x.Thickness, 0).Add(x => x.Duration, TimeSpan.FromMilliseconds(725))).Markup;
        var grid = context.Render<GridBackground>(p => p.Add(x => x.ChildContent, content).Add(x => x.CellSize, 999).Add(x => x.Opacity, 3).Add(x => x.Disabled, true)).Markup;
        var dots = context.Render<DotPattern>(p => p.Add(x => x.ChildContent, content).Add(x => x.Spacing, -1).Add(x => x.DotSize, 99)).Markup;
        var glow = context.Render<OrbitalGlow>(p => p.Add(x => x.ChildContent, content).Add(x => x.Intensity, 3)).Markup;

        glass.ShouldContain("--sc-fancy-glass-blur:64px");
        glass.ShouldContain("--sc-fancy-glass-border-opacity:0");
        beam.ShouldContain("--sc-fancy-border-beam-thickness:1px");
        beam.ShouldContain("--sc-fancy-duration:725ms");
        grid.ShouldContain("syntax-circus-fancy-grid-background__layer");
        grid.ShouldContain("aria-hidden=\"true\"");
        grid.ShouldContain("--sc-fancy-grid-cell-size:128px");
        grid.ShouldContain("data-fancy-disabled=\"true\"");
        dots.ShouldContain("--sc-fancy-dot-spacing:8px");
        dots.ShouldContain("--sc-fancy-dot-size:8px");
        glow.ShouldContain("syntax-circus-fancy-orbital-glow__layer");
        glow.ShouldContain("aria-hidden=\"true\"");
        glow.ShouldContain("--sc-fancy-orbital-glow-intensity:1");
        glow.ShouldContain("<a href=\"/next\">Continue</a>");
    }

    [Fact]
    public void NarrativeAndFeedbackEffects_PreserveSemanticsAndClampValues()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var scene = context.Render<ScrollScene>(p => p.Add(x => x.ChildContent, content).Add(x => x.Strength, 3).Add(x => x.Travel, 999)).Markup;
        var indicator = context.Render<ScrollIndicator>(p => p.Add(x => x.ChildContent, content).Add(x => x.Thickness, 0)).Markup;
        var backdrop = context.Render<ScrollBackdrop>(p => p.Add(x => x.ChildContent, content).Add(x => x.Intensity, -1)).Markup;
        var hover = context.Render<HoverLift>(p => p.Add(x => x.ChildContent, content).Add(x => x.Distance, 99).Add(x => x.Scale, 2)).Markup;
        var press = context.Render<PressScale>(p => p.Add(x => x.ChildContent, content).Add(x => x.Scale, 0)).Markup;
        var focus = context.Render<FocusHalo>(p => p.Add(x => x.ChildContent, content).Add(x => x.Opacity, 2).Add(x => x.Spread, 99)).Markup;

        scene.ShouldContain("data-fancy-scroll-scene=\"lift\"");
        scene.ShouldContain("--sc-fancy-scroll-scene-strength:1");
        scene.ShouldContain("--sc-fancy-scroll-scene-travel:300px");
        indicator.ShouldContain("syntax-circus-fancy-scroll-indicator__line");
        indicator.ShouldContain("aria-hidden=\"true\"");
        indicator.ShouldContain("--sc-fancy-scroll-indicator-thickness:1px");
        backdrop.ShouldContain("syntax-circus-fancy-scroll-backdrop__layer");
        backdrop.ShouldContain("aria-hidden=\"true\"");
        backdrop.ShouldContain("--sc-fancy-scroll-backdrop-intensity:0");
        hover.ShouldContain("--sc-fancy-hover-lift-distance:32px");
        hover.ShouldContain("--sc-fancy-hover-lift-scale:1.1");
        press.ShouldContain("--sc-fancy-press-scale:0.9");
        focus.ShouldContain("syntax-circus-fancy-focus-halo__halo");
        focus.ShouldContain("aria-hidden=\"true\"");
        focus.ShouldContain("--sc-fancy-focus-halo-opacity:1");
        focus.ShouldContain("--sc-fancy-focus-halo-spread:16px");
        focus.ShouldContain("<a href=\"/next\">Continue</a>");
    }

    [Fact]
    public void CssFirstCatalogAndPresets_PreserveSemanticsAndRequireNoRuntime()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<button type=\"button\">Save</button>");

        var stroke = context.Render<TextStroke>(p => p.Add(x => x.ChildContent, content).Add(x => x.Width, 99)).Markup;
        var highlight = context.Render<HighlightText>(p => p.Add(x => x.ChildContent, content).Add(x => x.Opacity, -1)).Markup;
        var gradient = context.Render<GradientDivider>(p => p.Add(x => x.Thickness, 0)).Markup;
        var wave = context.Render<WaveDivider>(p => p.Add(x => x.Amplitude, 99)).Markup;
        var section = context.Render<SectionDivider>(p => p.Add(x => x.Inset, -1)).Markup;
        var mesh = context.Render<MeshBackground>(p => p.Add(x => x.ChildContent, content).Add(x => x.Intensity, 2)).Markup;
        var corners = context.Render<CornerAccents>(p => p.Add(x => x.ChildContent, content).Add(x => x.Length, 0)).Markup;
        var paper = context.Render<PaperSurface>(p => p.Add(x => x.ChildContent, content).Add(x => x.TextureOpacity, 2)).Markup;
        var glow = context.Render<EdgeGlow>(p => p.Add(x => x.ChildContent, content).Add(x => x.Placement, EdgeGlowPlacement.End).Add(x => x.Size, 0)).Markup;
        var action = context.Render<ActionCard>(p => p.Add(x => x.ChildContent, content)).Markup;
        var reading = context.Render<ReadingSurface>(p => p.Add(x => x.ChildContent, content)).Markup;

        stroke.ShouldContain("--sc-fancy-text-stroke-width:8px");
        highlight.ShouldContain("--sc-fancy-highlight-opacity:0");
        gradient.ShouldContain("aria-hidden=\"true\"");
        gradient.ShouldContain("--sc-fancy-divider-thickness:1px");
        wave.ShouldContain("--sc-fancy-wave-amplitude:32px");
        section.ShouldContain("--sc-fancy-section-divider-inset:0px");
        mesh.ShouldContain("syntax-circus-fancy-mesh-background__layer");
        corners.ShouldContain("aria-hidden=\"true\"");
        paper.ShouldContain("syntax-circus-fancy-paper-surface__texture");
        glow.ShouldContain("data-fancy-placement=\"end\"");
        glow.ShouldContain("--sc-fancy-edge-glow-size:4px");
        action.ShouldContain("data-fancy-preset=\"action-card\"");
        action.ShouldContain("<button type=\"button\">Save</button>");
        reading.ShouldContain("data-fancy-preset=\"reading-surface\"");
        reading.ShouldContain("syntax-circus-fancy-grid-background");
    }

    [Fact]
    public void ThreeUiInspiredCatalog_PreservesSemanticsAndClampsPublicValues()
    {
        using var context = CreateContext();
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var constellation = context.Render<ConstellationBackground>(p => p.Add(x => x.ChildContent, content).Add(x => x.Density, 999).Add(x => x.LineOpacity, -1)).Markup;
        var flow = context.Render<ArcFlowBackground>(p => p.Add(x => x.ChildContent, content).Add(x => x.Density, 0).Add(x => x.Intensity, 2)).Markup;
        var neon = context.Render<NeonText>(p => p.Add(x => x.ChildContent, content).Add(x => x.Glow, 99).Add(x => x.StrokeWidth, -1)).Markup;
        var typeFlow = context.Render<TypeFlow>(p => p.Add(x => x.Text, "Readable heading").Add(x => x.Element, TypeFlowElement.Heading2).Add(x => x.Duration, TimeSpan.FromSeconds(-1))).Markup;
        var pulse = context.Render<StatusPulse>(p => p.Add(x => x.ChildContent, content).Add(x => x.Size, 99)).Markup;
        var halo = context.Render<LaunchHalo>(p => p.Add(x => x.ChildContent, content).Add(x => x.Intensity, -1).Add(x => x.Spread, 99)).Markup;

        constellation.ShouldContain("syntax-circus-fancy-constellation-background__canvas");
        constellation.ShouldContain("aria-hidden=\"true\"");
        constellation.ShouldContain("--sc-fancy-constellation-density:96");
        constellation.ShouldContain("--sc-fancy-constellation-line-opacity:0");
        flow.ShouldContain("syntax-circus-fancy-arc-flow-background__canvas");
        flow.ShouldContain("--sc-fancy-arc-flow-density:1");
        flow.ShouldContain("--sc-fancy-arc-flow-intensity:1");
        neon.ShouldContain("--sc-fancy-neon-text-glow:24px");
        neon.ShouldContain("--sc-fancy-neon-text-stroke-width:0px");
        typeFlow.ShouldContain("data-fancy-effect=\"type-flow\"");
        typeFlow.ShouldContain("<h2");
        typeFlow.ShouldContain("Readable heading");
        typeFlow.ShouldContain("--sc-fancy-duration:0ms");
        pulse.ShouldContain("syntax-circus-fancy-status-pulse__layer");
        pulse.ShouldContain("aria-hidden=\"true\"");
        pulse.ShouldContain("--sc-fancy-status-pulse-size:48px");
        halo.ShouldContain("syntax-circus-fancy-launch-halo__layer");
        halo.ShouldContain("aria-hidden=\"true\"");
        halo.ShouldContain("--sc-fancy-launch-halo-intensity:0");
        halo.ShouldContain("--sc-fancy-launch-halo-spread:64px");
        halo.ShouldContain("<a href=\"/next\">Continue</a>");
        context.Render<StatusPulse>(p => p.Add(x => x.ChildContent, content))
            .Find(".syntax-circus-fancy-status-pulse__content").InnerHtml.ShouldContain("syntax-circus-fancy-status-pulse__layer");
        context.Render<LaunchHalo>(p => p.Add(x => x.ChildContent, content))
            .Find(".syntax-circus-fancy-launch-halo__content").InnerHtml.ShouldContain("syntax-circus-fancy-launch-halo__layer");
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

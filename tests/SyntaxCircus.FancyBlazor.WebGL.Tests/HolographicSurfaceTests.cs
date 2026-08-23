using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text.Json;
using Xunit;

namespace SyntaxCircus.FancyBlazor.WebGL.Tests;

public sealed class HolographicSurfaceTests
{
    private const string ModulePath = "./_content/SyntaxCircus.FancyBlazor.WebGL/js/fancy-blazor-webgl.js";

    [Fact]
    public void HolographicSurface_MergesAttributesAndPreservesSemanticChildContent()
    {
        using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(1);
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var cut = context.Render<HolographicSurface>(parameters => parameters
            .Add(component => component.ChildContent, content)
            .Add(component => component.CssClass, "product-surface")
            .Add(component => component.Style, "margin:1rem")
            .Add(component => component.Intensity, 9)
            .Add(component => component.Depth, -1)
            .Add(component => component.Sheen, 9)
            .Add(component => component.Speed, -1)
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["class"] = "test-hook",
                ["style"] = "padding:1rem",
                ["aria-label"] = "Featured surface",
            }));
        var markup = cut.Markup;
        var style = cut.Find(".syntax-circus-fancy-holographic-surface").GetAttribute("style") ?? string.Empty;

        markup.ShouldContain("syntax-circus-fancy-holographic-surface product-surface test-hook");
        markup.ShouldContain("--sc-fancy-holographic-intensity:1");
        markup.ShouldContain("--sc-fancy-holographic-depth:0");
        markup.ShouldContain("--sc-fancy-holographic-sheen:1");
        markup.ShouldContain("--sc-fancy-holographic-speed:0");
        style.ShouldContain("margin:1rem");
        style.ShouldContain("padding:1rem");
        markup.ShouldContain("aria-label=\"Featured surface\"");
        markup.ShouldContain("<a href=\"/next\">Continue</a>");
        markup.ShouldContain("syntax-circus-fancy-holographic-surface__canvas");
        markup.ShouldContain("aria-hidden=\"true\"");
        markup.ShouldContain("tabindex=\"-1\"");
    }

    [Fact]
    public void HolographicSurface_DisablingAfterCreation_DestroysDecorativeRuntimeAndRetainsContent()
    {
        using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(7);
        module.SetupVoid("destroyEffect", _ => true);
        RenderFragment content = builder => builder.AddMarkupContent(0, "<button type=\"button\">Save</button>");

        var cut = context.Render<HolographicSurface>(parameters => parameters.Add(component => component.ChildContent, content));
        cut.Render(parameters => parameters.Add(component => component.Disabled, true));

        context.JSInterop.VerifyInvoke("import");
        module.VerifyInvoke("createEffect");
        module.VerifyInvoke("destroyEffect");
        cut.Markup.ShouldContain("data-fancy-disabled=\"true\"");
        cut.Markup.ShouldContain("<button type=\"button\">Save</button>");
    }

    [Fact]
    public void HolographicSurface_CreateCall_UsesSharedFancyBlazorDefaults()
    {
        using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(1);

        context.Render<HolographicSurface>(parameters => parameters.Add(component => component.ChildContent, builder => builder.AddContent(0, "Readable")));

        var invocation = module.Invocations.Single(call => call.Identifier == "createEffect");
        var defaults = JsonSerializer.Serialize(invocation.Arguments[3]);
        defaults.ShouldContain("\"motionPreference\":\"RespectSystem\"");
        defaults.ShouldContain("\"quality\":\"Auto\"");
        defaults.ShouldContain("\"pauseWhenHidden\":true");
        defaults.ShouldContain("\"pauseWhenOffscreen\":true");
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorWebGl();
        return context;
    }
}

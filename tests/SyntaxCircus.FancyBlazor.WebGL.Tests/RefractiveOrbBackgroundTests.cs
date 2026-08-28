using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text.Json;
using Xunit;

namespace SyntaxCircus.FancyBlazor.WebGL.Tests;

public sealed class RefractiveOrbBackgroundTests
{
    private const string ModulePath = "./_content/SyntaxCircus.FancyBlazor.WebGL/js/fancy-blazor-webgl.js";

    [Fact]
    public void RefractiveOrbBackground_MergesAttributesAndPreservesSemanticChildContent()
    {
        using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(1);
        RenderFragment content = builder => builder.AddMarkupContent(0, "<a href=\"/next\">Continue</a>");

        var cut = context.Render<RefractiveOrbBackground>(parameters => parameters
            .Add(component => component.ChildContent, content)
            .Add(component => component.CssClass, "product-surface")
            .Add(component => component.Style, "margin:1rem")
            .Add(component => component.Intensity, 9)
            .Add(component => component.Radius, -1)
            .Add(component => component.Distortion, 9)
            .Add(component => component.Sheen, -1)
            .Add(component => component.Speed, 9)
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["class"] = "test-hook",
                ["style"] = "padding:1rem",
                ["aria-label"] = "Featured surface",
            }));
        var markup = cut.Markup;
        var style = cut.Find(".syntax-circus-fancy-refractive-orb-background").GetAttribute("style") ?? string.Empty;

        markup.ShouldContain("syntax-circus-fancy-refractive-orb-background product-surface test-hook");
        markup.ShouldContain("--sc-fancy-refractive-orb-intensity:1");
        markup.ShouldContain("--sc-fancy-refractive-orb-radius:0");
        markup.ShouldContain("--sc-fancy-refractive-orb-distortion:1");
        markup.ShouldContain("--sc-fancy-refractive-orb-sheen:0");
        markup.ShouldContain("--sc-fancy-refractive-orb-speed:3");
        style.ShouldContain("margin:1rem");
        style.ShouldContain("padding:1rem");
        markup.ShouldContain("aria-label=\"Featured surface\"");
        markup.ShouldContain("<a href=\"/next\">Continue</a>");
        markup.ShouldContain("syntax-circus-fancy-refractive-orb-background__canvas");
        markup.ShouldContain("aria-hidden=\"true\"");
        markup.ShouldContain("tabindex=\"-1\"");
    }

    [Fact]
    public void RefractiveOrbBackground_DisablingAfterCreation_DestroysDecorativeRuntimeAndRetainsContent()
    {
        using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(7);
        module.SetupVoid("destroyEffect", _ => true);
        RenderFragment content = builder => builder.AddMarkupContent(0, "<button type=\"button\">Save</button>");

        var cut = context.Render<RefractiveOrbBackground>(parameters => parameters.Add(component => component.ChildContent, content));
        cut.Render(parameters => parameters.Add(component => component.Disabled, true));

        context.JSInterop.VerifyInvoke("import");
        module.VerifyInvoke("createEffect");
        module.VerifyInvoke("destroyEffect");
        cut.Markup.ShouldContain("data-fancy-disabled=\"true\"");
        cut.Markup.ShouldContain("<button type=\"button\">Save</button>");
    }

    [Fact]
    public void RefractiveOrbBackground_CreateCall_UsesSharedFancyBlazorDefaults()
    {
        using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(1);

        context.Render<RefractiveOrbBackground>(parameters => parameters.Add(component => component.ChildContent, builder => builder.AddContent(0, "Readable")));

        var invocation = module.Invocations.Single(call => call.Identifier == "createEffect");
        var defaults = JsonSerializer.Serialize(invocation.Arguments[3]);
        defaults.ShouldContain("\"motionPreference\":\"RespectSystem\"");
        defaults.ShouldContain("\"quality\":\"Auto\"");
        defaults.ShouldContain("\"pauseWhenHidden\":true");
        defaults.ShouldContain("\"pauseWhenOffscreen\":true");
    }

    [Fact]
    public async Task RefractiveOrbBackground_RapidReenable_WaitsForTeardownBeforeCreatingReplacement()
    {
        await using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(11);
        var destroy = module.SetupVoid("destroyEffect", _ => true);
        module.SetupVoid("disposeRuntime", _ => true).SetVoidResult();

        var cut = context.Render<RefractiveOrbBackground>();

        cut.Render(parameters => parameters.Add(component => component.Disabled, true));
        cut.Render(parameters => parameters.Add(component => component.Disabled, false));

        module.Invocations.Count(call => call.Identifier == "createEffect").ShouldBe(1);

        await cut.InvokeAsync(() => destroy.SetVoidResult());
        await cut.WaitForStateAsync(
            () => module.Invocations.Count(call => call.Identifier == "createEffect") == 2,
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task RefractiveOrbBackground_DisposalDuringCreation_DestroysTheStaleHandle()
    {
        await using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        var create = module.Setup<long>("createEffect", _ => true);
        module.SetupVoid("destroyEffect", _ => true).SetVoidResult();
        module.SetupVoid("disposeRuntime", _ => true).SetVoidResult();

        context.Render<RefractiveOrbBackground>();

        var disposal = context.DisposeComponentsAsync();
        create.SetResult(17);
        await disposal.WaitAsync(TimeSpan.FromSeconds(1), Xunit.TestContext.Current.CancellationToken);

        module.Invocations.Count(call => call.Identifier == "destroyEffect").ShouldBe(1);
        module.Invocations.Single(call => call.Identifier == "destroyEffect").Arguments[0].ShouldBe(17L);
    }

    [Fact]
    public async Task RefractiveOrbBackground_DisposalDuringTeardown_DoesNotCreateOrRenderAReplacement()
    {
        await using var context = CreateContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<long>("createEffect", _ => true).SetResult(23);
        var destroy = module.SetupVoid("destroyEffect", _ => true);
        module.SetupVoid("disposeRuntime", _ => true).SetVoidResult();
        var cut = context.Render<RefractiveOrbBackground>();

        cut.Render(parameters => parameters.Add(component => component.Disabled, true));
        var disposal = context.DisposeComponentsAsync();
        destroy.SetVoidResult();
        await disposal.WaitAsync(TimeSpan.FromSeconds(1), Xunit.TestContext.Current.CancellationToken);

        module.Invocations.Count(call => call.Identifier == "createEffect").ShouldBe(1);
        module.Invocations.Count(call => call.Identifier == "destroyEffect").ShouldBe(1);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorWebGl();
        return context;
    }
}

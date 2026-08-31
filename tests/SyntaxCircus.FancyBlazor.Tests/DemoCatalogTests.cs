using Bunit;
using FancyBlazor.Demo.Client.Pages;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.Tests;

public sealed class DemoCatalogTests
{
    private static readonly string[] DemoDestinations =
    [
        "/background", "/expanded-effects", "/threeui-inspiration", "/core-kinetic-catalog", "/atmospheric-backgrounds",
        "/border", "/spatial-surfaces", "/webgl",
        "/reveal", "/tilt", "/narrative-motion",
        "/expressive-effects", "/css-first-catalog", "/kinetic-text",
        "/interaction-feedback",
        "/composition-authoring",
        "/ui-companion", "/marketing",
    ];

    [Fact]
    public void Home_CatalogDirectory_LinksToEveryDemoDestination()
    {
        using var context = new BunitContext();
        context.Services.AddFancyBlazor();
        var module = context.JSInterop.SetupModule("./_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js");
        module.Setup<long>("createEffect", _ => true).SetResult(1);
        module.SetupVoid("updateEffect", _ => true);
        module.SetupVoid("destroyEffect", _ => true);

        var cut = context.Render<Home>();

        var directory = cut.Find("[data-testid='catalog-directory']");
        directory.QuerySelectorAll("a[href]").Select(link => link.GetAttribute("href"))
            .ShouldBe(DemoDestinations);
    }

    [Fact]
    public void Home_CatalogDirectory_LabelsWebGlCompanionAsPreview()
    {
        using var context = new BunitContext();
        context.Services.AddFancyBlazor();
        var module = context.JSInterop.SetupModule("./_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js");
        module.Setup<long>("createEffect", _ => true).SetResult(1);
        module.SetupVoid("destroyEffect", _ => true);

        var cut = context.Render<Home>();

        cut.Find("[data-testid='catalog-directory'] a[href='/webgl']")
            .TextContent.ShouldContain("Preview");
    }

    [Fact]
    public void WebGlShowcase_IdentifiesCompanionAsPreviewAndShowsPackageSetup()
    {
        using var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorWebGl();
        var module = context.JSInterop.SetupModule("./_content/SyntaxCircus.FancyBlazor.WebGL/js/fancy-blazor-webgl.js");
        module.Setup<long>("createEffect", _ => true).SetResult(1);
        module.SetupVoid("destroyEffect", _ => true);
        module.SetupVoid("disposeRuntime", _ => true);

        var cut = context.Render<WebGlShowcase>();

        cut.Find("[data-testid='webgl-preview-status']").TextContent.ShouldContain("Preview");
        cut.Markup.ShouldContain("SyntaxCircus.FancyBlazor.WebGL");
        cut.Markup.ShouldContain("AddFancyBlazorWebGl");
    }
}

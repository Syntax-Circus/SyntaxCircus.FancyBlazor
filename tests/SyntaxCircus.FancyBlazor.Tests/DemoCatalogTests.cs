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
        "/background", "/expanded-effects", "/threeui-inspiration",
        "/border", "/spatial-surfaces",
        "/reveal", "/tilt", "/narrative-motion",
        "/expressive-effects", "/css-first-catalog",
        "/interaction-feedback", "/composition-authoring",
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
}

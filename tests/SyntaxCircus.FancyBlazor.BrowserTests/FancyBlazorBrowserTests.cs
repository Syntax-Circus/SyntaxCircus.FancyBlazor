using Microsoft.Playwright;
using NCrunch.Framework;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.BrowserTests;

/// <summary>
/// Browser integration tests share an expensive Kestrel and Chromium fixture.
/// Keep them in one NCrunch execution task rather than reinitializing it per test.
/// </summary>
[Atomic]
public sealed class FancyBlazorBrowserTests(BrowserHostFixture fixture) : IClassFixture<BrowserHostFixture>
{
    [Fact]
    public async Task Home_StaticResponse_ContainsSemanticContentAndLocalAssets()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync(fixture.TestHostUrl, TestContext.Current.CancellationToken);

        html.ShouldContain("Make the ordinary interface catch light.");
        html.ShouldContain("data-testid=\"shader-background\"");
        html.ShouldContain("_framework/blazor.web.js");
        html.ShouldNotContain("shader.gallery/cdn");
        html.ShouldNotContain("react");
    }

    [Fact]
    public async Task Home_InteractiveRuntime_InitializesAllJavaScriptEffects()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(fixture.TestHostUrl);
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 4");

        var state = await page.Locator("[data-testid='shader-background']").GetAttributeAsync("data-fancy-state");
        var count = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor.instanceCount");

        state.ShouldBe("active");
        count.ShouldBeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task ShaderBackground_WithReducedMotion_UsesStaticStateAndKeepsContent()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(fixture.TestHostUrl);
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=shader-background]')?.dataset.fancyState === 'reduced'");

        (await page.Locator("h1").InnerTextAsync()).ShouldContain("catch light");
        (await page.Locator("[data-testid='shader-background']").GetAttributeAsync("data-fancy-state")).ShouldBe("reduced");
    }

    [Fact]
    public async Task ShaderBackground_WithoutWebGl_FallsBackWithoutLosingContent()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        await context.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorDisableWebGl = true;");
        var page = await context.NewPageAsync();
        await page.GotoAsync(fixture.TestHostUrl);
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=shader-background]')?.dataset.fancyState === 'fallback'");

        (await page.Locator("h1").InnerTextAsync()).ShouldContain("catch light");
        (await page.Locator("a.primary-action").IsEnabledAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task ShaderBackground_Offscreen_StopsAndRestartsItsAnimationFrame()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 900, Height = 500 },
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/background");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.getDiagnostics().animationFrameCount === 1");

        var stage = page.Locator("[data-testid='background-example']");
        await stage.EvaluateAsync("element => element.style.transform = 'translateY(2000px)'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount === 0");

        await stage.EvaluateAsync("element => element.style.removeProperty('transform')");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount === 1");
    }

    [Fact]
    public async Task Tilt_PointerMove_ChangesDecorativeStateWithoutBreakingLink()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/tilt");
        var tilt = page.Locator("[data-testid='tilt-example']");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=tilt-example]')?.dataset.fancyState === 'ready'");
        await tilt.EvaluateAsync("element => element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, clientX: 120, clientY: 80 }))");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=tilt-example]')?.dataset.fancyEngaged === 'true'");

        (await tilt.GetAttributeAsync("tabindex")).ShouldBeNull();
        (await tilt.Locator("a").GetAttributeAsync("href")).ShouldBe("/");
    }

    [Fact]
    public async Task Reveal_EntersVisibleStateWithoutAriaHidingContent()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/reveal");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 3");
        var reveal = page.Locator("[data-fancy-effect='reveal']").First;
        await reveal.ScrollIntoViewIfNeededAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-fancy-effect=reveal]')?.dataset.fancyVisible === 'true'");

        (await reveal.GetAttributeAsync("aria-hidden")).ShouldBeNull();
    }

    [Fact]
    public async Task RevealDemo_ReplayButton_RecreatesTheSequencedExamples()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/reveal");
        var examples = page.Locator("[data-testid='reveal-examples']");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=reveal-examples]')?.dataset.replayRun === '0'");
        await page.Locator("[data-testid='replay-reveal']").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=reveal-examples]')?.dataset.replayRun === '1'");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-fancy-effect=reveal]')?.dataset.fancyVisible === 'true'");
    }

    [Fact]
    public async Task ExpressiveEffects_EnhanceTextAndReleasePointerEffects()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/expressive-effects");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 3");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=text-reveal-example]')?.dataset.fancyReady === 'true'");

        var text = page.Locator("[data-testid='text-reveal-example']");
        (await text.GetAttributeAsync("aria-label")).ShouldBe("Accessible animated heading");
        (await text.Locator(".syntax-circus-fancy-text-reveal__token").CountAsync()).ShouldBeGreaterThan(0);
        await page.Locator("[data-testid='ripple-example'] button").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.syntax-circus-fancy-ripple__wave').length > 0");
        await page.Locator("[data-testid='cursor-trail-example']").HoverAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount > 0");
        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForFunctionAsync("() => (globalThis.__syntaxCircusFancyBlazor?.instanceCount ?? 0) === 0");
    }

    [Fact]
    public async Task EnhancedNavigation_TwentyCycles_ReleasesEveryEffect()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(fixture.TestHostUrl);
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 4");

        for (var cycle = 0; cycle < 20; cycle++)
        {
            await page.Locator("header a[href='/border']").ClickAsync();
            await page.WaitForURLAsync("**/border");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 0");
            await page.Locator("a.wordmark").ClickAsync();
            await page.WaitForURLAsync(fixture.TestHostUrl + "/");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 4");
        }

        await page.Locator("header a[href='/border']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 0");
    }

    [Fact]
    public async Task ReducedMotionPages_ProduceNonEmptyDeterministicVisualArtifacts()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ReducedMotion = ReducedMotion.Reduce,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 },
        });
        var page = await context.NewPageAsync();
        var artifactDirectory = Path.Combine(Environment.CurrentDirectory, "TestResults", "visual");
        Directory.CreateDirectory(artifactDirectory);

        foreach (var route in new[] { "/background", "/border", "/reveal", "/tilt" })
        {
            await page.GotoAsync(fixture.TestHostUrl + route);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForTimeoutAsync(350);
            var screenshot = await page.Locator("main").ScreenshotAsync(new LocatorScreenshotOptions
            {
                Path = Path.Combine(artifactDirectory, route.TrimStart('/') + ".png"),
            });
            screenshot.Length.ShouldBeGreaterThan(1_000, $"visual artifact for {route}");
            screenshot.Take(8).ShouldBe(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        }
    }

    [Fact]
    public async Task StandaloneWebAssemblyHost_LoadsAndInitializesEffects()
    {
        using var process = BrowserHostFixture.StartStandaloneHost(out var standaloneUrl);
        try
        {
            await BrowserHostFixture.WaitUntilReadyAsync(standaloneUrl, process);
            await using var context = await fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync(standaloneUrl);
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 3");

            (await page.Locator("h1").InnerTextAsync()).ShouldBe("Standalone WebAssembly");
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(10_000);
            }
        }
    }
}

using Microsoft.Playwright;
using NCrunch.Framework;
using Shouldly;
using Xunit;
using FancyBlazor.Demo;

namespace SyntaxCircus.FancyBlazor.BrowserTests;

/// <summary>
/// Browser integration tests share an expensive Kestrel and Chromium fixture.
/// Keep them in one NCrunch execution task rather than reinitializing it per test.
/// </summary>
[Atomic]
public sealed class FancyBlazorBrowserTests(BrowserHostFixture fixture) : IClassFixture<BrowserHostFixture>
{
    [Fact]
    public async Task HolographicSurface_StaticResponse_PreservesFallbackAndSemanticChild()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync($"{fixture.TestHostUrl}/webgl", TestContext.Current.CancellationToken);

        html.ShouldContain("Holographic semantic content");
        html.ShouldContain("syntax-circus-fancy-holographic-surface");
        html.ShouldNotContain("data-webgl-state=\"active\"");
    }

    [Fact]
    public async Task HolographicSurface_LoadsThreeOnlyWhenVisibleAndActive()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = 900, Height = 1 } });
        var page = await context.NewPageAsync();
        var threeRequests = 0;
        var coreStatus = 0;
        page.Request += (_, request) =>
        {
            if (request.Url.Contains("/vendor/three/build/three.module", StringComparison.Ordinal))
            {
                threeRequests++;
            }
        };
        page.Response += (_, response) =>
        {
            if (response.Url.Contains("/vendor/three/build/three.core", StringComparison.Ordinal))
            {
                coreStatus = response.Status;
            }
        };

        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForTimeoutAsync(150);
        threeRequests.ShouldBe(0);
        await page.Locator("[data-testid='holographic-first']").ScrollIntoViewIfNeededAsync();
        await page.WaitForTimeoutAsync(750);

        threeRequests.ShouldBe(1);
        coreStatus.ShouldBe(200);
        (await page.EvaluateAsync<string?>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().lastFailure")).ShouldBeNull();
        (await page.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeTrue();
        (await page.Locator("[data-testid='holographic-first']").GetAttributeAsync("data-webgl-state")).ShouldBe("active");
    }

    [Fact]
    public async Task HolographicSurface_UsesFinePointerOnlyAndUpdatesParametersWithoutReplacingChild()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 2");

        var surface = page.Locator("[data-testid='holographic-first']");
        await surface.EvaluateAsync("element => element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerType: 'mouse', clientX: 80, clientY: 40 }))");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-first]')?.dataset.webglPointer === 'true'");
        await page.Locator("[data-testid='holographic-update']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.intensity === 0.8)");
        await page.Locator("[data-testid='holographic-palette']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.palette?.[0] === '#60a5fa')");

        await surface.Locator("button").FocusAsync();
        (await surface.Locator("button").GetAttributeAsync("tabindex")).ShouldBeNull();
        (await surface.Locator("button").EvaluateAsync<bool>("element => document.activeElement === element")).ShouldBeTrue();
        await surface.Locator("button").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-activation]')?.textContent === 'Activated'");
        (await page.Locator("[data-testid='holographic-activation']").InnerTextAsync()).ShouldBe("Activated");
    }

    [Fact]
    public async Task HolographicSurface_ReducedMotionAndForcedFailure_KeepFallbackWithoutLoadingThree()
    {
        await using var reducedContext = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await reducedPage.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await reducedPage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 5");
        (await reducedPage.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeFalse();
        (await reducedPage.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount")).ShouldBe(0);

        await using var failureContext = await fixture.Browser.NewContextAsync();
        await failureContext.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlForceFailure = true;");
        var failurePage = await failureContext.NewPageAsync();
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await failurePage.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-first]')?.dataset.webglState === 'fallback'");
        (await failurePage.Locator("[data-testid='holographic-first'] article").InnerTextAsync()).ShouldBe("Holographic semantic content");
    }

    [Fact]
    public async Task HolographicSurface_ReleasesContextsForOffscreenHiddenAndContextLoss_AndPromotesFifoWaiters()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = 900, Height = 700 } });
        await context.AddInitScriptAsync("Object.defineProperty(document, 'hidden', { configurable: true, writable: true, value: false });");
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 2");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().waitingContexts === 3");

        var first = page.Locator("[data-testid='holographic-first']");
        await first.EvaluateAsync("element => element.style.transform = 'translateY(3000px)'");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-third]')?.dataset.webglState === 'active'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.state === 'waiting')?.handle > 3");
        await page.Locator("[data-testid='holographic-third'] canvas").EvaluateAsync("canvas => canvas.dispatchEvent(new Event('webglcontextlost', { cancelable: true }))");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts < 2");
        await page.Locator("[data-testid='holographic-third'] canvas").EvaluateAsync("canvas => canvas.dispatchEvent(new Event('webglcontextrestored'))");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-third]')?.dataset.webglState === 'active'");

        await page.EvaluateAsync("() => { document.hidden = true; document.dispatchEvent(new Event('visibilitychange')); }");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0 && globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount === 0");
        await page.EvaluateAsync("() => { document.hidden = false; document.dispatchEvent(new Event('visibilitychange')); }");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 2");
    }

    [Fact]
    public async Task HolographicSurface_TwentyNavigationCycles_DisposeEveryContext()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();

        for (var cycle = 0; cycle < 20; cycle++)
        {
            await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 2");
            await page.Locator("header a[href='/border']").ClickAsync();
            await page.WaitForURLAsync("**/border");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instanceCount === 0");
        }
    }

    [Fact]
    public async Task HolographicSurface_ReleasesAnInFlightConstructionBeforeItCanStart()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        await context.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlRendererDelayMs = 2000;");
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-first]')?.dataset.webglState === 'loading'");

        await page.Locator("header a[href='/border']").ClickAsync();
        await page.WaitForURLAsync("**/border");
        await page.WaitForTimeoutAsync(2200);

        await page.WaitForFunctionAsync("() => { const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics(); return diagnostics.instanceCount === 0 && diagnostics.activeContexts === 0 && diagnostics.liveRendererCount === 0 && diagnostics.rendererObjectsCreated === diagnostics.rendererObjectsDestroyed; }");
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().rendererObjectsCreated"))
            .ShouldBe(await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().rendererObjectsDestroyed"));
    }

    [Fact]
    public async Task HolographicSurface_RejectsPointerUpdatesOnCoarsePointers()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        await context.AddInitScriptAsync("{ const original = window.matchMedia; window.matchMedia = query => query === '(pointer: fine)' ? { matches: false } : original(query); }");
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 2");

        await page.Locator("[data-testid='holographic-first']").EvaluateAsync("element => element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerType: 'mouse', clientX: 80, clientY: 40 }))");

        (await page.Locator("[data-testid='holographic-first']").GetAttributeAsync("data-webgl-pointer")).ShouldBeNull();
    }

    [Fact]
    public async Task InteractiveAutoDemo_LoadsTheWebGlCompanionAfterWebAssemblyHydration()
    {
        using var process = BrowserHostFixture.StartServerHost(typeof(DemoAssemblyMarker).Assembly.Location, out var demoUrl);
        try
        {
            await BrowserHostFixture.WaitUntilReadyAsync(demoUrl, process);
            await using var context = await fixture.Browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync($"{demoUrl}/test-webgl-auto");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");

            (await page.Locator("[data-testid='auto-holographic'] article").InnerTextAsync()).ShouldBe("Interactive Auto holographic content");
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

    [Fact]
    public async Task CoreOnlyPage_NeverRequestsCompanionAssets()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var companionRequests = 0;
        page.Request += (_, request) =>
        {
            if (request.Url.Contains("/_content/SyntaxCircus.FancyBlazor.WebGL/", StringComparison.Ordinal))
            {
                companionRequests++;
            }
        };

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        companionRequests.ShouldBe(0);
    }

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
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 3");
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
    public async Task SpatialSurfaces_PreserveSemanticControlsAndReducedMotionStaticState()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/spatial-surfaces");

        var grid = page.Locator("[data-testid='spatial-grid-example']");
        (await grid.Locator("a").GetAttributeAsync("href")).ShouldBe("/background");
        (await grid.Locator(".syntax-circus-fancy-grid-background__layer").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.EvaluateAsync<bool>("() => matchMedia('(prefers-reduced-motion: reduce)').matches")).ShouldBeTrue();
        (await page.Locator("[data-testid='spatial-beam-example']").GetAttributeAsync("data-fancy-animated")).ShouldBe("true");
        (await page.Locator("[data-testid='dot-pattern-example'] .syntax-circus-fancy-dot-pattern__layer").GetAttributeAsync("aria-hidden")).ShouldBe("true");
    }

    [Fact]
    public async Task NarrativeMotion_TracksVisibleProgressAndKeepsDecorationsHidden()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/narrative-motion");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=scroll-scene-example]')?.dataset.fancyReady === 'true'");

        var scene = page.Locator("[data-testid='scroll-scene-example']");
        var progress = await scene.GetAttributeAsync("data-fancy-scroll-progress");
        int.Parse(progress!).ShouldBeInRange(0, 100);
        (await page.Locator("[data-testid='scroll-backdrop-example'] .syntax-circus-fancy-scroll-backdrop__layer").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.Locator("[data-testid='scroll-indicator-example'] .syntax-circus-fancy-scroll-indicator__line").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await scene.Locator("a").GetAttributeAsync("href")).ShouldBe("/border");

        await scene.EvaluateAsync("element => element.style.transform = 'translateY(2000px)'");
        await page.WaitForTimeoutAsync(150);
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount")).ShouldBe(0);
    }

    [Fact]
    public async Task InteractionFeedback_RespondsWithoutReplacingSemantics()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/interaction-feedback");

        var hover = page.Locator("[data-testid='hover-lift-example']");
        await hover.HoverAsync();
        (await hover.Locator(".syntax-circus-fancy-hover-lift__content").EvaluateAsync<string>("element => getComputedStyle(element).transform")).ShouldNotBe("none");

        var press = page.Locator("[data-testid='press-scale-example']");
        await press.Locator("button").FocusAsync();
        await page.Keyboard.DownAsync("Enter");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=press-scale-example]')?.dataset.fancyPressed === 'true'");
        await page.Keyboard.UpAsync("Enter");
        await page.WaitForFunctionAsync("() => !document.querySelector('[data-testid=press-scale-example]')?.dataset.fancyPressed");

        var focus = page.Locator("[data-testid='focus-halo-example']");
        await focus.Locator("a").FocusAsync();
        (await focus.GetAttributeAsync("tabindex")).ShouldBeNull();
        (await focus.Locator("a").GetAttributeAsync("href")).ShouldBe("/border");
        (await focus.Locator("a").EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle")).ShouldNotBe("none");
        await page.WaitForFunctionAsync("() => Number.parseFloat(getComputedStyle(document.querySelector('[data-testid=focus-halo-example] .syntax-circus-fancy-focus-halo__halo')).opacity) > 0");
        (await focus.Locator(".syntax-circus-fancy-focus-halo__halo").EvaluateAsync<float>("element => Number.parseFloat(getComputedStyle(element).opacity)")).ShouldBeGreaterThan(0);
        (await focus.Locator(".syntax-circus-fancy-focus-halo__halo").GetAttributeAsync("aria-hidden")).ShouldBe("true");

        var field = page.Locator("[data-testid='focus-halo-input-example']");
        await field.Locator("input").FocusAsync();
        await page.WaitForFunctionAsync("() => Number.parseFloat(getComputedStyle(document.querySelector('[data-testid=focus-halo-input-example] .syntax-circus-fancy-focus-halo__halo')).opacity) > 0");
        (await field.Locator("input").InputValueAsync()).ShouldBeEmpty();
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
    public async Task CssFirstCatalog_UsesStaticDecorationsWithoutRuntimeInstances()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/css-first-catalog");

        (await page.Locator("[data-testid='text-stroke-example'] h1").InnerTextAsync()).ShouldBe("Outlined semantic heading");
        (await page.Locator("[data-testid='gradient-divider-example']").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.Locator("[data-testid='wave-divider-example']").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.Locator("[data-testid='mesh-background-example'] .syntax-circus-fancy-mesh-background__layer").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.Locator("[data-testid='corner-accents-example'] [aria-hidden='true']").CountAsync()).ShouldBe(2);
        (await page.Locator("[data-testid='edge-glow-example'] a").GetAttributeAsync("href")).ShouldBe("/border");
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount ?? 0")).ShouldBe(0);
    }

    [Fact]
    public async Task ThreeUiInspiredCatalog_UsesBoundedCanvasAndPreservesSemanticControls()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/threeui-inspiration");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 3");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=type-flow-example]')?.dataset.fancyReady === 'true'");

        var constellation = page.Locator("[data-testid='constellation-example']");
        var flow = page.Locator("[data-testid='arc-flow-example']");
        (await constellation.Locator("canvas").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await flow.Locator("canvas").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.Locator("[data-testid='type-flow-example']").InnerTextAsync()).ShouldBe("Text that arrives with restraint.");
        (await page.Locator("[data-testid='status-pulse-example'] button").IsEnabledAsync()).ShouldBeTrue();
        (await page.Locator("[data-testid='launch-halo-example'] a").GetAttributeAsync("href")).ShouldBe("/border");

        await constellation.EvaluateAsync("element => element.style.transform = 'translateY(3000px)'");
        await flow.EvaluateAsync("element => element.style.transform = 'translateY(3000px)'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount === 0");
    }

    [Fact]
    public async Task ThreeUiInspiredCatalog_WithReducedMotion_UsesStaticCanvasFallbacks()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/threeui-inspiration");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 3");

        (await page.Locator("[data-testid='constellation-example'] article").InnerTextAsync()).ShouldBe("Constellation content");
        (await page.Locator("[data-testid='arc-flow-example'] canvas").EvaluateAsync<string>("element => getComputedStyle(element).display")).ShouldBe("none");
        (await page.Locator("[data-testid='type-flow-example']").InnerTextAsync()).ShouldBe("Text that arrives with restraint.");
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount")).ShouldBe(0);
    }

    [Fact]
    public async Task ThreeUiInspiredCatalog_WithoutCanvasContext_KeepsStaticContentAndFallbackBackground()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        await context.AddInitScriptAsync("HTMLCanvasElement.prototype.getContext = () => null;");
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/threeui-inspiration");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 3");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=type-flow-example]')?.dataset.fancyReady === 'true'");

        var constellation = page.Locator("[data-testid='constellation-example']");
        (await constellation.Locator("article").InnerTextAsync()).ShouldBe("Constellation content");
        (await constellation.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor")).ShouldNotBe("rgba(0, 0, 0, 0)");
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount")).ShouldBe(0);
    }

    [Fact]
    public async Task CompositionPresets_KeepSemanticControlsAndOnlyPressScaleInitializesRuntime()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/composition-authoring");
        await page.WaitForFunctionAsync("() => (globalThis.__syntaxCircusFancyBlazor?.instanceCount ?? 0) === 1");

        (await page.Locator("[data-testid='aurora-hero-example'] article").InnerTextAsync()).ShouldBe("Aurora content");
        (await page.Locator("[data-testid='reading-surface-example'] article").InnerTextAsync()).ShouldBe("Reading content");
        var action = page.Locator("[data-testid='action-card-example']");
        await action.Locator("button").FocusAsync();
        (await action.GetAttributeAsync("tabindex")).ShouldBeNull();
        await action.Locator("button").PressAsync("Enter");
        (await page.Locator("[data-testid='feature-panel-example'] a").GetAttributeAsync("href")).ShouldBe("/border");
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

        foreach (var route in new[] { "/background", "/border", "/reveal", "/tilt", "/spatial-surfaces", "/css-first-catalog", "/composition-authoring", "/threeui-inspiration" })
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
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");

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

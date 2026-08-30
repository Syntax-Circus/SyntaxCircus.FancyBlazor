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
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = 900, Height = 1 } });
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
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

        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
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
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'holographic-first' && instance.active && instance.renderer)");

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
    public async Task HolographicSurface_PreservesZeroSpeedAtCreationAndUpdate()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'holographic-first' && instance.active && instance.renderer)");

        await page.WaitForTimeoutAsync(150);
        (await page.EvaluateAsync<double>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.time"))
            .ShouldBe(0d);

        await page.Locator("[data-testid='holographic-speed']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.time > 0");
        await page.Locator("[data-testid='holographic-speed']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.time === 0");
        await page.WaitForTimeoutAsync(150);
        (await page.EvaluateAsync<double>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.time"))
            .ShouldBe(0d);
    }

    [Fact]
    public async Task HolographicSurface_ReconcilesLiveInteractionAndAvoidsUnchangedBufferResizes()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'holographic-first' && instance.active && instance.renderer)");

        var surface = page.Locator("[data-testid='holographic-first']");
        var initialResizes = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.resizeCount");
        await page.WaitForTimeoutAsync(150);
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.resizeCount"))
            .ShouldBe(initialResizes);
        (await page.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.usesNormalizedUv"))
            .ShouldBeTrue();

        await page.Locator("[data-testid='holographic-interactive']").ClickAsync();
        await page.WaitForFunctionAsync("() => { const element = document.querySelector('[data-testid=holographic-first]'); delete element.dataset.webglPointer; element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerType: 'mouse', clientX: 80, clientY: 40 })); return !element.dataset.webglPointer; }");
        await page.Locator("[data-testid='holographic-interactive']").ClickAsync();
        await page.WaitForFunctionAsync("() => { const element = document.querySelector('[data-testid=holographic-first]'); element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerType: 'mouse', clientX: 80, clientY: 40 })); return element.dataset.webglPointer === 'true'; }");

        await surface.EvaluateAsync("element => element.style.width = '420px'");
        await page.WaitForFunctionAsync($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'holographic-first').renderer.resizeCount > {initialResizes}");
    }

    [Fact]
    public async Task HolographicSurface_ReactsToLiveReducedMotionAndLogsAsyncConstructionFailureOnce()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        await context.AddInitScriptAsync("""
            (() => {
                let reduced = false;
                const listeners = new Set();
                const media = { get matches() { return reduced; }, addEventListener: (_, listener) => listeners.add(listener), removeEventListener: (_, listener) => listeners.delete(listener) };
                const original = window.matchMedia.bind(window);
                window.matchMedia = query => query === '(prefers-reduced-motion: reduce)' ? media : original(query);
                globalThis.__setReducedMotion = value => { reduced = value; for (const listener of listeners) { listener({ matches: reduced }); } };
            })();
            """);
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await page.WaitForFunctionAsync("() => typeof globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics === 'function' && globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
        await page.EvaluateAsync("() => globalThis.__setReducedMotion(true)");
        await page.WaitForFunctionAsync("() => { const d = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics(); return d.activeContexts === 0 && d.animationFrameCount === 0 && d.instances.every(instance => instance.state === 'reduced'); }");
        await page.Locator("[data-testid='holographic-first']").EvaluateAsync("element => { delete element.dataset.webglPointer; element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerType: 'mouse', clientX: 80, clientY: 40 })); }");
        (await page.Locator("[data-testid='holographic-first']").GetAttributeAsync("data-webgl-pointer")).ShouldBeNull();
        await page.EvaluateAsync("() => globalThis.__setReducedMotion(false)");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
        await page.Locator("[data-testid='holographic-first']").EvaluateAsync("element => element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerType: 'mouse', clientX: 80, clientY: 40 }))");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-first]')?.dataset.webglPointer === 'true'");

        await using var failureContext = await browser.NewContextAsync();
        var warningCount = 0;
        var failurePage = await failureContext.NewPageAsync();
        await using var failureCleanup = new WebGlPageCleanup(failurePage);
        failurePage.Console += (_, message) => { if (message.Type == "warning" && message.Text.Contains("CSS fallback remains active", StringComparison.Ordinal)) warningCount++; };
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await failurePage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.active && instance.renderer)");
        var failureTestId = await failurePage.EvaluateAsync<string>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.active && instance.renderer).testId");
        await failurePage.EvaluateAsync("() => document.querySelectorAll('[data-fancy-effect=holographic-surface]').forEach(element => element.style.display = 'none')");
        await failurePage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0");
        await failurePage.EvaluateAsync("testId => { const surface = document.querySelector(`[data-testid='${testId}']`); surface.querySelector('canvas').getContext = () => null; surface.style.display = 'block'; }", failureTestId);
        await failurePage.WaitForFunctionAsync("() => { const d = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics(); return d.threeLoaded && d.lastFailure && d.instances.some(instance => instance.state === 'fallback'); }");
        warningCount.ShouldBe(1);
    }

    [Fact]
    public async Task HolographicSurface_ReducedMotionAndForcedFailure_KeepFallbackWithoutLoadingThree()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var reducedContext = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await using var reducedCleanup = new WebGlPageCleanup(reducedPage);
        await reducedPage.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await reducedPage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 5");
        (await reducedPage.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeFalse();
        (await reducedPage.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount")).ShouldBe(0);

        await using var failureContext = await browser.NewContextAsync();
        await failureContext.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlForceFailure = true;");
        var failurePage = await failureContext.NewPageAsync();
        await using var failureCleanup = new WebGlPageCleanup(failurePage);
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await failurePage.WaitForFunctionAsync("() => document.querySelector('[data-testid=holographic-first]')?.dataset.webglState === 'fallback'");
        (await failurePage.Locator("[data-testid='holographic-first'] article").InnerTextAsync()).ShouldContain("Holographic semantic content");
        var fallback = failurePage.Locator("[data-testid='holographic-first']");
        (await fallback.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"))
            .ShouldNotBe("rgba(0, 0, 0, 0)");
        (await fallback.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"))
            .StartsWith("radial-gradient", StringComparison.Ordinal).ShouldBeTrue();
        var fallbackButton = failurePage.Locator("[data-testid='holographic-first'] button");
        (await fallbackButton.CountAsync()).ShouldBe(1);
        (await fallbackButton.IsEnabledAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task HolographicSurface_ReleasesContextsForOffscreenHiddenAndContextLoss_AndPromotesFifoWaiters()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new ViewportSize { Width = 900, Height = 700 } });
        await context.AddInitScriptAsync("Object.defineProperty(document, 'hidden', { configurable: true, writable: true, value: false });");
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 2");
        await page.WaitForFunctionAsync("() => { const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics(); return diagnostics.waitingHandles?.length === 3 && diagnostics.instances.every(instance => instance.testId); }");

        var activeTestId = await page.EvaluateAsync<string>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.active).testId");
        var firstWaiterHandle = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().waitingHandles[0]");
        var firstWaiterTestId = await page.EvaluateAsync<string>($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.handle === {firstWaiterHandle}).testId");
        var offscreenWaiterHandle = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().waitingHandles.at(-1)");
        var offscreenWaiterTestId = await page.EvaluateAsync<string>($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.handle === {offscreenWaiterHandle}).testId");

        await page.Locator($"[data-testid='{offscreenWaiterTestId}']").EvaluateAsync("element => element.style.display = 'none'");
        await page.WaitForFunctionAsync($"() => !globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().waitingHandles.includes({offscreenWaiterHandle})");

        await page.Locator($"[data-testid='{activeTestId}']").EvaluateAsync("element => element.style.display = 'none'");
        await page.WaitForFunctionAsync($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.handle === {firstWaiterHandle})?.active === true");

        var nextWaiterHandle = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().waitingHandles[0]");
        var promotedCanvas = page.Locator($"[data-testid='{firstWaiterTestId}'] canvas");
        await promotedCanvas.EvaluateAsync("canvas => { const context = canvas.getContext('webgl2'); globalThis.__syntaxCircusFancyBlazorWebGlLostContext = context.getExtension('WEBGL_lose_context'); globalThis.__syntaxCircusFancyBlazorWebGlLostContext.loseContext(); }");
        await page.WaitForFunctionAsync($"() => {{ const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics(); const lost = diagnostics.instances.find(instance => instance.handle === {firstWaiterHandle}); return lost?.active === false && lost.state === 'fallback' && diagnostics.instances.find(instance => instance.handle === {nextWaiterHandle})?.active === true; }}");

        await page.EvaluateAsync("() => globalThis.__syntaxCircusFancyBlazorWebGlLostContext.restoreContext()");
        await page.WaitForFunctionAsync($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().waitingHandles.at(-1) === {firstWaiterHandle}");

        var otherTestIds = await page.EvaluateAsync<string[]>($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.filter(instance => instance.handle !== {firstWaiterHandle}).map(instance => instance.testId)");
        foreach (var testId in otherTestIds)
        {
            await page.Locator($"[data-testid='{testId}']").EvaluateAsync("element => element.style.display = 'none'");
        }
        await page.WaitForFunctionAsync($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.handle === {firstWaiterHandle})?.active === true");

        await page.EvaluateAsync("() => { document.hidden = true; document.dispatchEvent(new Event('visibilitychange')); }");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0 && globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount === 0");
        await page.EvaluateAsync("() => { document.hidden = false; document.dispatchEvent(new Event('visibilitychange')); }");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
    }

    [Fact]
    public async Task HolographicSurface_TwentyNavigationCycles_DisposeEveryContext()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);

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
    public async Task HolographicSurface_Pagehide_DisposesEveryRuntimeResourceBeforeThePageCloses()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'holographic-first' && instance.active && instance.renderer)");
        await page.EvaluateAsync("""
            () => addEventListener('pagehide', () => {
                const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics();
                sessionStorage.setItem('webgl-pagehide-diagnostics', JSON.stringify({
                    instances: diagnostics.instanceCount,
                    active: diagnostics.activeContexts,
                    waiting: diagnostics.waitingContexts,
                    frames: diagnostics.animationFrameCount,
                    liveRenderers: diagnostics.liveRendererCount,
                }));
            }, { once: true })
            """);

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForFunctionAsync("""
            () => {
                const diagnostics = JSON.parse(sessionStorage.getItem('webgl-pagehide-diagnostics') ?? 'null');
                return diagnostics && diagnostics.instances === 0 && diagnostics.active === 0 && diagnostics.waiting === 0 && diagnostics.frames === 0 && diagnostics.liveRenderers === 0;
            }
            """);
    }

    [Fact]
    public async Task HolographicSurface_ReleasesAnInFlightConstructionBeforeItCanStart()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        await context.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlConstructionGate = new Promise(resolve => { globalThis.__syntaxCircusFancyBlazorWebGlResolveConstructionGate = resolve; });");
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts > 0 && document.querySelector('[data-webgl-state=loading]')");

        await page.Locator("header a[href='/border']").ClickAsync();
        await page.WaitForURLAsync("**/border");
        await page.EvaluateAsync("() => globalThis.__syntaxCircusFancyBlazorWebGlResolveConstructionGate()");

        await page.WaitForFunctionAsync("() => { const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics(); return diagnostics.instanceCount === 0 && diagnostics.activeContexts === 0 && diagnostics.liveRendererCount === 0 && diagnostics.rendererObjectsCreated === diagnostics.rendererObjectsDestroyed; }");
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().rendererObjectsCreated"))
            .ShouldBe(await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().rendererObjectsDestroyed"));
    }

    [Fact]
    public async Task HolographicSurface_RejectsPointerUpdatesOnCoarsePointers()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        await context.AddInitScriptAsync("{ const original = window.matchMedia; window.matchMedia = query => query === '(pointer: fine)' ? { matches: false } : original(query); }");
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl?single=true");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 1");

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
            await using var browser = await NewWebGlBrowserAsync();
            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await using var webGlCleanup = new WebGlPageCleanup(page);
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
    public async Task InteractiveAutoDemo_WebGlShowcaseControlsUpdateSurfaceWithoutReplacingContent()
    {
        using var process = BrowserHostFixture.StartServerHost(typeof(DemoAssemblyMarker).Assembly.Location, out var demoUrl);
        try
        {
            await BrowserHostFixture.WaitUntilReadyAsync(demoUrl, process);
            await using var browser = await NewWebGlBrowserAsync();
            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await using var webGlCleanup = new WebGlPageCleanup(page);
            await page.GotoAsync($"{demoUrl}/webgl");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'webgl-showcase-surface' && instance.active && instance.renderer)");

            await page.EvaluateAsync("() => { globalThis.__webGlShowcaseContent = document.querySelector('[data-testid=webgl-showcase-surface] article'); globalThis.__webGlShowcaseCanvas = document.querySelector('[data-testid=webgl-showcase-surface] canvas'); }");
            var initialHandle = await page.EvaluateAsync<long>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'webgl-showcase-surface').handle");
            await page.Locator("[data-testid='webgl-intensity']").FillAsync("0.82");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'webgl-showcase-surface')?.intensity === 0.82");

            var surface = page.Locator("[data-testid='webgl-showcase-surface']");
            var style = await surface.GetAttributeAsync("style");
            style.ShouldNotBeNull();
            style.ShouldContain("--sc-fancy-holographic-intensity:0.82");
            (await page.EvaluateAsync<bool>("() => globalThis.__webGlShowcaseContent === document.querySelector('[data-testid=webgl-showcase-surface] article')")).ShouldBeTrue();

            await page.Locator("[data-testid='webgl-preset-deep-field']").ClickAsync(new() { Timeout = 1_000 });
            // The preset button now sits far below the four additional WebGL workbenches, so clicking it
            // scrolls the Holographic specimen off-screen and its runtime legitimately pauses (pause-when-offscreen).
            // Scroll it back into view so the reactivated renderer reflects the already-updated preset options.
            await surface.ScrollIntoViewIfNeededAsync();
            await page.WaitForFunctionAsync("() => { const instance = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(candidate => candidate.testId === 'webgl-showcase-surface'); return instance?.renderer && instance.palette?.[0] === '#10b981'; }");
            var presetStyle = await surface.GetAttributeAsync("style");
            presetStyle.ShouldNotBeNull();
            presetStyle.ShouldContain("--sc-fancy-holographic-depth:0.86");
            (await page.EvaluateAsync<long>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'webgl-showcase-surface').handle")).ShouldBe(initialHandle);
            (await page.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'webgl-showcase-surface')?.active === true")).ShouldBeTrue();
            (await page.EvaluateAsync<bool>("() => globalThis.__webGlShowcaseContent === document.querySelector('[data-testid=webgl-showcase-surface] article')")).ShouldBeTrue();

            await page.Locator("[data-testid='webgl-disabled']").CheckAsync();
            await page.WaitForFunctionAsync("() => !globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.testId === 'webgl-showcase-surface')");
            (await surface.GetAttributeAsync("data-fancy-disabled")).ShouldBe("true");
            (await surface.Locator("article").InnerTextAsync()).ShouldContain("Semantic HTML");
            (await page.EvaluateAsync<bool>("() => globalThis.__webGlShowcaseContent === document.querySelector('[data-testid=webgl-showcase-surface] article')")).ShouldBeTrue();

            await page.Locator("[data-testid='webgl-disabled']").UncheckAsync();
            await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=webgl-showcase-surface]')?.dataset.fancyDisabled === 'false'");
            (await page.EvaluateAsync<bool>("() => globalThis.__webGlShowcaseCanvas !== document.querySelector('[data-testid=webgl-showcase-surface] canvas')")).ShouldBeTrue();
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.testId === 'webgl-showcase-surface' && instance.active && instance.renderer)");
            (await page.EvaluateAsync<bool>("() => { const canvas = document.querySelector('[data-testid=webgl-showcase-surface] canvas'); const context = canvas?.getContext('webgl2'); return Boolean(context && !context.isContextLost()); }")).ShouldBeTrue();
            (await page.EvaluateAsync<bool>("() => globalThis.__webGlShowcaseContent === document.querySelector('[data-testid=webgl-showcase-surface] article')")).ShouldBeTrue();
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
    public async Task WaveFieldBackground_StaticResponse_PreservesFallbackAndSemanticChild()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync($"{fixture.TestHostUrl}/webgl-wave-field", TestContext.Current.CancellationToken);

        html.ShouldContain("Wave field semantic content");
        html.ShouldContain("syntax-circus-fancy-wave-field-background");
        html.ShouldNotContain("data-webgl-state=\"active\"");
    }

    [Fact]
    public async Task WaveFieldBackground_UpdatesParametersWithoutReplacingChildContent()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-wave-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'wave-field-first' && instance.active && instance.renderer)");

        await page.EvaluateAsync("() => { globalThis.__waveFieldContent = document.querySelector('[data-testid=wave-field-first] article'); }");
        await page.Locator("[data-testid='wave-field-update']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.renderer?.amplitude === 0.9)");
        (await page.EvaluateAsync<bool>("() => globalThis.__waveFieldContent === document.querySelector('[data-testid=wave-field-first] article')")).ShouldBeTrue();

        var surface = page.Locator("[data-testid='wave-field-first']");
        await surface.Locator("button").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=wave-field-activation]')?.textContent === 'Activated'");
        (await page.Locator("[data-testid='wave-field-activation']").InnerTextAsync()).ShouldBe("Activated");
    }

    [Fact]
    public async Task WaveFieldBackground_ReducedMotionAndForcedFailure_KeepFallbackWithoutLoadingThree()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var reducedContext = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await using var reducedCleanup = new WebGlPageCleanup(reducedPage);
        await reducedPage.GotoAsync($"{fixture.TestHostUrl}/webgl-wave-field");
        await reducedPage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 1");
        (await reducedPage.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeFalse();
        (await reducedPage.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount")).ShouldBe(0);

        await using var failureContext = await browser.NewContextAsync();
        await failureContext.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlForceFailure = true;");
        var failurePage = await failureContext.NewPageAsync();
        await using var failureCleanup = new WebGlPageCleanup(failurePage);
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl-wave-field");
        await failurePage.WaitForFunctionAsync("() => document.querySelector('[data-testid=wave-field-first]')?.dataset.webglState === 'fallback'");
        (await failurePage.Locator("[data-testid='wave-field-first'] article").InnerTextAsync()).ShouldContain("Wave field semantic content");
        var fallback = failurePage.Locator("[data-testid='wave-field-first']");
        (await fallback.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"))
            .StartsWith("repeating-linear-gradient", StringComparison.Ordinal).ShouldBeTrue();
    }

    [Fact]
    public async Task WaveFieldBackground_ReleasesContextForHiddenAndRestoresWhenVisible()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-wave-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");

        await page.Locator("[data-testid='wave-field-first']").EvaluateAsync("element => element.style.display = 'none'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0");

        await page.Locator("[data-testid='wave-field-first']").EvaluateAsync("element => element.style.display = 'block'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
    }

    [Fact]
    public async Task WaveFieldBackground_RepeatedNavigationCycles_DisposeEveryContext()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);

        for (var cycle = 0; cycle < 5; cycle++)
        {
            await page.GotoAsync($"{fixture.TestHostUrl}/webgl-wave-field");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");
            await page.Locator("header a[href='/border']").ClickAsync();
            await page.WaitForURLAsync("**/border");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instanceCount === 0");
        }
    }

    [Fact]
    public async Task WaveFieldBackground_Pagehide_DisposesEveryRuntimeResourceBeforeThePageCloses()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-wave-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'wave-field-first' && instance.active && instance.renderer)");
        await page.EvaluateAsync("""
            () => addEventListener('pagehide', () => {
                const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics();
                sessionStorage.setItem('wave-field-pagehide-diagnostics', JSON.stringify({
                    instances: diagnostics.instanceCount,
                    active: diagnostics.activeContexts,
                    waiting: diagnostics.waitingContexts,
                    frames: diagnostics.animationFrameCount,
                    liveRenderers: diagnostics.liveRendererCount,
                }));
            }, { once: true })
            """);

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForFunctionAsync("""
            () => {
                const diagnostics = JSON.parse(sessionStorage.getItem('wave-field-pagehide-diagnostics') ?? 'null');
                return diagnostics && diagnostics.instances === 0 && diagnostics.active === 0 && diagnostics.waiting === 0 && diagnostics.frames === 0 && diagnostics.liveRenderers === 0;
            }
            """);
    }

    [Fact]
    public async Task RefractiveOrbBackground_StaticResponse_PreservesFallbackAndSemanticChild()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync($"{fixture.TestHostUrl}/webgl-refractive-orb", TestContext.Current.CancellationToken);

        html.ShouldContain("Refractive orb semantic content");
        html.ShouldContain("syntax-circus-fancy-refractive-orb-background");
        html.ShouldNotContain("data-webgl-state=\"active\"");
    }

    [Fact]
    public async Task RefractiveOrbBackground_UpdatesParametersWithoutReplacingChildContent()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-refractive-orb");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'refractive-orb-first' && instance.active && instance.renderer)");

        await page.EvaluateAsync("() => { globalThis.__refractiveOrbContent = document.querySelector('[data-testid=refractive-orb-first] article'); }");
        await page.Locator("[data-testid='refractive-orb-update']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.renderer?.distortion === 0.9)");
        (await page.EvaluateAsync<bool>("() => globalThis.__refractiveOrbContent === document.querySelector('[data-testid=refractive-orb-first] article')")).ShouldBeTrue();

        var surface = page.Locator("[data-testid='refractive-orb-first']");
        await surface.Locator("button").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=refractive-orb-activation]')?.textContent === 'Activated'");
        (await page.Locator("[data-testid='refractive-orb-activation']").InnerTextAsync()).ShouldBe("Activated");
    }

    [Fact]
    public async Task RefractiveOrbBackground_ReducedMotionAndForcedFailure_KeepFallbackWithoutLoadingThree()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var reducedContext = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await using var reducedCleanup = new WebGlPageCleanup(reducedPage);
        await reducedPage.GotoAsync($"{fixture.TestHostUrl}/webgl-refractive-orb");
        await reducedPage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 1");
        (await reducedPage.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeFalse();
        (await reducedPage.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount")).ShouldBe(0);

        await using var failureContext = await browser.NewContextAsync();
        await failureContext.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlForceFailure = true;");
        var failurePage = await failureContext.NewPageAsync();
        await using var failureCleanup = new WebGlPageCleanup(failurePage);
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl-refractive-orb");
        await failurePage.WaitForFunctionAsync("() => document.querySelector('[data-testid=refractive-orb-first]')?.dataset.webglState === 'fallback'");
        (await failurePage.Locator("[data-testid='refractive-orb-first'] article").InnerTextAsync()).ShouldContain("Refractive orb semantic content");
        var fallback = failurePage.Locator("[data-testid='refractive-orb-first']");
        (await fallback.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"))
            .StartsWith("radial-gradient", StringComparison.Ordinal).ShouldBeTrue();
    }

    [Fact]
    public async Task RefractiveOrbBackground_ReleasesContextForHiddenAndRestoresWhenVisible()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-refractive-orb");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");

        await page.Locator("[data-testid='refractive-orb-first']").EvaluateAsync("element => element.style.display = 'none'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0");

        await page.Locator("[data-testid='refractive-orb-first']").EvaluateAsync("element => element.style.display = 'block'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
    }

    [Fact]
    public async Task RefractiveOrbBackground_RepeatedNavigationCycles_DisposeEveryContext()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);

        for (var cycle = 0; cycle < 5; cycle++)
        {
            await page.GotoAsync($"{fixture.TestHostUrl}/webgl-refractive-orb");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");
            await page.Locator("header a[href='/border']").ClickAsync();
            await page.WaitForURLAsync("**/border");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instanceCount === 0");
        }
    }

    [Fact]
    public async Task RefractiveOrbBackground_Pagehide_DisposesEveryRuntimeResourceBeforeThePageCloses()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-refractive-orb");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'refractive-orb-first' && instance.active && instance.renderer)");
        await page.EvaluateAsync("""
            () => addEventListener('pagehide', () => {
                const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics();
                sessionStorage.setItem('refractive-orb-pagehide-diagnostics', JSON.stringify({
                    instances: diagnostics.instanceCount,
                    active: diagnostics.activeContexts,
                    waiting: diagnostics.waitingContexts,
                    frames: diagnostics.animationFrameCount,
                    liveRenderers: diagnostics.liveRendererCount,
                }));
            }, { once: true })
            """);

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForFunctionAsync("""
            () => {
                const diagnostics = JSON.parse(sessionStorage.getItem('refractive-orb-pagehide-diagnostics') ?? 'null');
                return diagnostics && diagnostics.instances === 0 && diagnostics.active === 0 && diagnostics.waiting === 0 && diagnostics.frames === 0 && diagnostics.liveRenderers === 0;
            }
            """);
    }

    [Fact]
    public async Task PrismFieldBackground_StaticResponse_PreservesFallbackAndSemanticChild()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync($"{fixture.TestHostUrl}/webgl-prism-field", TestContext.Current.CancellationToken);

        html.ShouldContain("Prism field semantic content");
        html.ShouldContain("syntax-circus-fancy-prism-field-background");
        html.ShouldNotContain("data-webgl-state=\"active\"");
    }

    [Fact]
    public async Task PrismFieldBackground_UpdatesParametersWithoutReplacingChildContent()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-prism-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'prism-field-first' && instance.active && instance.renderer)");

        await page.EvaluateAsync("() => { globalThis.__prismFieldContent = document.querySelector('[data-testid=prism-field-first] article'); }");
        await page.Locator("[data-testid='prism-field-update']").ClickAsync();
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.renderer?.dispersion === 0.9)");
        (await page.EvaluateAsync<bool>("() => globalThis.__prismFieldContent === document.querySelector('[data-testid=prism-field-first] article')")).ShouldBeTrue();

        var surface = page.Locator("[data-testid='prism-field-first']");
        await surface.Locator("button").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=prism-field-activation]')?.textContent === 'Activated'");
        (await page.Locator("[data-testid='prism-field-activation']").InnerTextAsync()).ShouldBe("Activated");
    }

    [Fact]
    public async Task PrismFieldBackground_ReducedMotionAndForcedFailure_KeepFallbackWithoutLoadingThree()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var reducedContext = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await using var reducedCleanup = new WebGlPageCleanup(reducedPage);
        await reducedPage.GotoAsync($"{fixture.TestHostUrl}/webgl-prism-field");
        await reducedPage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 1");
        (await reducedPage.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeFalse();
        (await reducedPage.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount")).ShouldBe(0);

        await using var failureContext = await browser.NewContextAsync();
        await failureContext.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlForceFailure = true;");
        var failurePage = await failureContext.NewPageAsync();
        await using var failureCleanup = new WebGlPageCleanup(failurePage);
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl-prism-field");
        await failurePage.WaitForFunctionAsync("() => document.querySelector('[data-testid=prism-field-first]')?.dataset.webglState === 'fallback'");
        (await failurePage.Locator("[data-testid='prism-field-first'] article").InnerTextAsync()).ShouldContain("Prism field semantic content");
        var fallback = failurePage.Locator("[data-testid='prism-field-first']");
        (await fallback.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"))
            .StartsWith("conic-gradient", StringComparison.Ordinal).ShouldBeTrue();
    }

    [Fact]
    public async Task PrismFieldBackground_ReleasesContextForHiddenAndRestoresWhenVisible()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-prism-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");

        await page.Locator("[data-testid='prism-field-first']").EvaluateAsync("element => element.style.display = 'none'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0");

        await page.Locator("[data-testid='prism-field-first']").EvaluateAsync("element => element.style.display = 'block'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
    }

    [Fact]
    public async Task PrismFieldBackground_RepeatedNavigationCycles_DisposeEveryContext()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);

        for (var cycle = 0; cycle < 5; cycle++)
        {
            await page.GotoAsync($"{fixture.TestHostUrl}/webgl-prism-field");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");
            await page.Locator("header a[href='/border']").ClickAsync();
            await page.WaitForURLAsync("**/border");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instanceCount === 0");
        }
    }

    [Fact]
    public async Task PrismFieldBackground_Pagehide_DisposesEveryRuntimeResourceBeforeThePageCloses()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-prism-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'prism-field-first' && instance.active && instance.renderer)");
        await page.EvaluateAsync("""
            () => addEventListener('pagehide', () => {
                const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics();
                sessionStorage.setItem('prism-field-pagehide-diagnostics', JSON.stringify({
                    instances: diagnostics.instanceCount,
                    active: diagnostics.activeContexts,
                    waiting: diagnostics.waitingContexts,
                    frames: diagnostics.animationFrameCount,
                    liveRenderers: diagnostics.liveRendererCount,
                }));
            }, { once: true })
            """);

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForFunctionAsync("""
            () => {
                const diagnostics = JSON.parse(sessionStorage.getItem('prism-field-pagehide-diagnostics') ?? 'null');
                return diagnostics && diagnostics.instances === 0 && diagnostics.active === 0 && diagnostics.waiting === 0 && diagnostics.frames === 0 && diagnostics.liveRenderers === 0;
            }
            """);
    }

    [Fact]
    public async Task ParticleFieldBackground_StaticResponse_PreservesFallbackAndSemanticChild()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync($"{fixture.TestHostUrl}/webgl-particle-field", TestContext.Current.CancellationToken);

        html.ShouldContain("Particle field semantic content");
        html.ShouldContain("syntax-circus-fancy-particle-field-background");
        html.ShouldNotContain("data-webgl-state=\"active\"");
    }

    [Fact]
    public async Task ParticleFieldBackground_UpdatesParticleCountWithoutReplacingChildContent()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-particle-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'particle-field-first' && instance.active && instance.renderer)");

        var initialCount = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.find(instance => instance.testId === 'particle-field-first').renderer.particleCount");
        await page.EvaluateAsync("() => { globalThis.__particleFieldContent = document.querySelector('[data-testid=particle-field-first] article'); }");
        await page.Locator("[data-testid='particle-field-update']").ClickAsync();
        await page.WaitForFunctionAsync($"() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instances.some(instance => instance.renderer?.particleCount > {initialCount})");
        (await page.EvaluateAsync<bool>("() => globalThis.__particleFieldContent === document.querySelector('[data-testid=particle-field-first] article')")).ShouldBeTrue();

        var surface = page.Locator("[data-testid='particle-field-first']");
        await surface.Locator("button").ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=particle-field-activation]')?.textContent === 'Activated'");
        (await page.Locator("[data-testid='particle-field-activation']").InnerTextAsync()).ShouldBe("Activated");
    }

    [Fact]
    public async Task ParticleFieldBackground_ReducedMotionAndForcedFailure_KeepFallbackWithoutLoadingThree()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var reducedContext = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await using var reducedCleanup = new WebGlPageCleanup(reducedPage);
        await reducedPage.GotoAsync($"{fixture.TestHostUrl}/webgl-particle-field");
        await reducedPage.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instanceCount === 1");
        (await reducedPage.EvaluateAsync<bool>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().threeLoaded")).ShouldBeFalse();
        (await reducedPage.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().animationFrameCount")).ShouldBe(0);

        await using var failureContext = await browser.NewContextAsync();
        await failureContext.AddInitScriptAsync("globalThis.__syntaxCircusFancyBlazorWebGlForceFailure = true;");
        var failurePage = await failureContext.NewPageAsync();
        await using var failureCleanup = new WebGlPageCleanup(failurePage);
        await failurePage.GotoAsync($"{fixture.TestHostUrl}/webgl-particle-field");
        await failurePage.WaitForFunctionAsync("() => document.querySelector('[data-testid=particle-field-first]')?.dataset.webglState === 'fallback'");
        (await failurePage.Locator("[data-testid='particle-field-first'] article").InnerTextAsync()).ShouldContain("Particle field semantic content");
        var fallback = failurePage.Locator("[data-testid='particle-field-first']");
        (await fallback.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"))
            .StartsWith("radial-gradient", StringComparison.Ordinal).ShouldBeTrue();
    }

    [Fact]
    public async Task ParticleFieldBackground_ReleasesContextForHiddenAndRestoresWhenVisible()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-particle-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");

        await page.Locator("[data-testid='particle-field-first']").EvaluateAsync("element => element.style.display = 'none'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 0");

        await page.Locator("[data-testid='particle-field-first']").EvaluateAsync("element => element.style.display = 'block'");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().activeContexts === 1");
    }

    [Fact]
    public async Task ParticleFieldBackground_RepeatedNavigationCycles_DisposeEveryContext()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);

        for (var cycle = 0; cycle < 5; cycle++)
        {
            await page.GotoAsync($"{fixture.TestHostUrl}/webgl-particle-field");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().activeContexts === 1");
            await page.Locator("header a[href='/border']").ClickAsync();
            await page.WaitForURLAsync("**/border");
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics().instanceCount === 0");
        }
    }

    [Fact]
    public async Task ParticleFieldBackground_Pagehide_DisposesEveryRuntimeResourceBeforeThePageCloses()
    {
        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await using var webGlCleanup = new WebGlPageCleanup(page);
        await page.GotoAsync($"{fixture.TestHostUrl}/webgl-particle-field");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.testId === 'particle-field-first' && instance.active && instance.renderer)");
        await page.EvaluateAsync("""
            () => addEventListener('pagehide', () => {
                const diagnostics = globalThis.__syntaxCircusFancyBlazorWebGl.getDiagnostics();
                sessionStorage.setItem('particle-field-pagehide-diagnostics', JSON.stringify({
                    instances: diagnostics.instanceCount,
                    active: diagnostics.activeContexts,
                    waiting: diagnostics.waitingContexts,
                    frames: diagnostics.animationFrameCount,
                    liveRenderers: diagnostics.liveRendererCount,
                }));
            }, { once: true })
            """);

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForFunctionAsync("""
            () => {
                const diagnostics = JSON.parse(sessionStorage.getItem('particle-field-pagehide-diagnostics') ?? 'null');
                return diagnostics && diagnostics.instances === 0 && diagnostics.active === 0 && diagnostics.waiting === 0 && diagnostics.frames === 0 && diagnostics.liveRenderers === 0;
            }
            """);
    }

    [Fact]
    public async Task CoreOnlyPage_NeverRequestsCompanionAssets()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        var companionRequests = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Url.Contains("/_content/SyntaxCircus.FancyBlazor.WebGL/", StringComparison.Ordinal))
            {
                companionRequests.Add(request.Url);
            }
        };

        await page.GotoAsync($"{fixture.TestHostUrl}/border");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        companionRequests.ShouldBeEmpty();
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
    public async Task UiCompanion_CoexistsCleanlyWithBootstrap5Reboot()
    {
        var withoutBootstrap = await GetUiCompanionComputedStylesAsync(bootstrap: false);
        var withBootstrap = await GetUiCompanionComputedStylesAsync(bootstrap: true);

        withBootstrap.BootstrapStylesheetLoaded.ShouldBeTrue();
        withoutBootstrap.BootstrapStylesheetLoaded.ShouldBeFalse();

        withBootstrap.ButtonBackgroundColor.ShouldBe(withoutBootstrap.ButtonBackgroundColor);
        withBootstrap.ButtonTextDecoration.ShouldBe(withoutBootstrap.ButtonTextDecoration);
        withBootstrap.LinkColor.ShouldBe(withoutBootstrap.LinkColor);
        withBootstrap.LinkTextDecorationLine.ShouldBe(withoutBootstrap.LinkTextDecorationLine);
        withBootstrap.BadgeBorderRadius.ShouldBe(withoutBootstrap.BadgeBorderRadius);
        withBootstrap.CardBackgroundColor.ShouldBe(withoutBootstrap.CardBackgroundColor);
        withBootstrap.NavbarBackgroundColor.ShouldBe(withoutBootstrap.NavbarBackgroundColor);
        withBootstrap.LogoCloudListStyleType.ShouldBe(withoutBootstrap.LogoCloudListStyleType);
        withBootstrap.TestimonialBackgroundColor.ShouldBe(withoutBootstrap.TestimonialBackgroundColor);
        withBootstrap.CallToActionBackgroundColor.ShouldBe(withoutBootstrap.CallToActionBackgroundColor);
        withBootstrap.FeatureGridDisplay.ShouldBe(withoutBootstrap.FeatureGridDisplay);
        withBootstrap.HeroHeadingFontSize.ShouldBe(withoutBootstrap.HeroHeadingFontSize);
        withBootstrap.PricingTableBorderCollapse.ShouldBe(withoutBootstrap.PricingTableBorderCollapse);
        withBootstrap.FaqAccordionTriggerFont.ShouldBe(withoutBootstrap.FaqAccordionTriggerFont);
    }

    [Fact]
    public async Task FaqAccordion_ClickTogglesAndEnforcesSingleOpen()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/ui-companion-bootstrap?bootstrap=false");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var accordion = page.Locator("[data-testid='faq-accordion-example']");
        var buttons = accordion.Locator("button");
        var first = buttons.Nth(0);
        var second = buttons.Nth(1);
        await first.WaitForAsync();

        (await first.GetAttributeAsync("aria-expanded")).ShouldBe("false");

        await first.ClickAsync();
        (await first.GetAttributeAsync("aria-expanded")).ShouldBe("true");
        var firstPanelId = await first.GetAttributeAsync("aria-controls");
        (await page.Locator($"#{firstPanelId}").IsHiddenAsync()).ShouldBeFalse();

        await second.ClickAsync();
        (await second.GetAttributeAsync("aria-expanded")).ShouldBe("true");
        (await first.GetAttributeAsync("aria-expanded")).ShouldBe("false");
        (await page.Locator($"#{firstPanelId}").IsHiddenAsync()).ShouldBeTrue();

        await second.ClickAsync();
        (await second.GetAttributeAsync("aria-expanded")).ShouldBe("false");
    }

    [Fact]
    public async Task FaqAccordion_KeyboardActivation_TogglesTrigger()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/ui-companion-bootstrap?bootstrap=false");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var trigger = page.Locator("[data-testid='faq-accordion-example'] button").First;
        await trigger.WaitForAsync();

        await trigger.FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        (await trigger.GetAttributeAsync("aria-expanded")).ShouldBe("true");

        await page.Keyboard.PressAsync("Space");
        (await trigger.GetAttributeAsync("aria-expanded")).ShouldBe("false");
    }

    [Fact]
    public async Task FaqAccordion_ExpandingDoesNotChangeContainerWidth()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/ui-companion-bootstrap?bootstrap=false");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var accordion = page.Locator("[data-testid='faq-accordion-width-example'] .syntax-circus-fancy-ui-faq-accordion");
        await accordion.WaitForAsync();
        var collapsedBox = await accordion.BoundingBoxAsync();
        collapsedBox.ShouldNotBeNull();

        var longTrigger = page.Locator("[data-testid='faq-accordion-width-example'] button").Nth(1);
        await longTrigger.ClickAsync();
        (await longTrigger.GetAttributeAsync("aria-expanded")).ShouldBe("true");

        var expandedBox = await accordion.BoundingBoxAsync();
        expandedBox.ShouldNotBeNull();
        expandedBox!.Width.ShouldBe(collapsedBox!.Width, 0.5);
    }

    [Fact]
    public async Task FaqAccordion_Animated_TransitionsHeightOnToggle()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/ui-companion-bootstrap?bootstrap=false");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var animatedPanel = page.Locator("[data-testid='faq-accordion-animated-example'] .syntax-circus-fancy-ui-faq-accordion__panel");
        var plainPanel = page.Locator("[data-testid='faq-accordion-example'] .syntax-circus-fancy-ui-faq-accordion__panel").First;
        await animatedPanel.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        (await animatedPanel.EvaluateAsync<string>("element => getComputedStyle(element).transitionDuration")).ShouldNotBe("0s");
        (await plainPanel.EvaluateAsync<string>("element => getComputedStyle(element).transitionDuration")).ShouldBe("0s");

        var trigger = page.Locator("[data-testid='faq-accordion-animated-example'] button").First;
        await trigger.ClickAsync();
        (await trigger.GetAttributeAsync("aria-expanded")).ShouldBe("true");
        await page.WaitForTimeoutAsync(300);
        (await animatedPanel.IsHiddenAsync()).ShouldBeFalse();

        await trigger.ClickAsync();
        (await trigger.GetAttributeAsync("aria-expanded")).ShouldBe("false");
        await page.WaitForTimeoutAsync(300);
        (await animatedPanel.IsHiddenAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task Marquee_TrackAndContentResistShrinkingForSeamlessLoop()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/core-kinetic-catalog");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var track = page.Locator("[data-testid='marquee-example'] .syntax-circus-fancy-marquee__track");
        var content = page.Locator("[data-testid='marquee-example'] .syntax-circus-fancy-marquee__content").First;
        await track.WaitForAsync();

        (await track.EvaluateAsync<string>("element => getComputedStyle(element).flexShrink")).ShouldBe("0");
        (await content.EvaluateAsync<string>("element => getComputedStyle(element).whiteSpace")).ShouldBe("nowrap");
    }

    private async Task<UiCompanionComputedStyles> GetUiCompanionComputedStylesAsync(bool bootstrap)
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/ui-companion-bootstrap?bootstrap={(bootstrap ? "true" : "false")}");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.Locator("[data-testid='fancy-button-example'] button").WaitForAsync();

        return new UiCompanionComputedStyles(
            BootstrapStylesheetLoaded: await page.Locator("[data-testid='bootstrap-stylesheet']").CountAsync() == 1,
            ButtonBackgroundColor: await page.Locator("[data-testid='fancy-button-example'] button").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"),
            ButtonTextDecoration: await page.Locator("[data-testid='fancy-button-example'] button").EvaluateAsync<string>("element => getComputedStyle(element).textDecorationLine"),
            LinkColor: await page.Locator("[data-testid='fancy-link-example'] a").EvaluateAsync<string>("element => getComputedStyle(element).color"),
            LinkTextDecorationLine: await page.Locator("[data-testid='fancy-link-example'] a").EvaluateAsync<string>("element => getComputedStyle(element).textDecorationLine"),
            BadgeBorderRadius: await page.Locator("[data-testid='fancy-badge-example'] span").EvaluateAsync<string>("element => getComputedStyle(element).borderRadius"),
            CardBackgroundColor: await page.Locator("[data-testid='fancy-card-example'] article").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"),
            NavbarBackgroundColor: await page.Locator("[data-testid='fancy-navbar-example'] nav").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"),
            LogoCloudListStyleType: await page.Locator("[data-testid='logo-cloud-example'] ul").EvaluateAsync<string>("element => getComputedStyle(element).listStyleType"),
            TestimonialBackgroundColor: await page.Locator("[data-testid='testimonial-example'] figure").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"),
            CallToActionBackgroundColor: await page.Locator("[data-testid='cta-example'] > div").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"),
            FeatureGridDisplay: await page.Locator("[data-testid='feature-grid-example'] ul").EvaluateAsync<string>("element => getComputedStyle(element).display"),
            HeroHeadingFontSize: await page.Locator("[data-testid='hero-example'] .syntax-circus-fancy-ui-hero__heading").EvaluateAsync<string>("element => getComputedStyle(element).fontSize"),
            PricingTableBorderCollapse: await page.Locator("[data-testid='pricing-table-example'] table").EvaluateAsync<string>("element => getComputedStyle(element).borderCollapse"),
            FaqAccordionTriggerFont: await page.Locator("[data-testid='faq-accordion-example'] button").First.EvaluateAsync<string>("element => getComputedStyle(element).fontWeight"));
    }

    private sealed record UiCompanionComputedStyles(
        bool BootstrapStylesheetLoaded,
        string ButtonBackgroundColor,
        string ButtonTextDecoration,
        string LinkColor,
        string LinkTextDecorationLine,
        string BadgeBorderRadius,
        string CardBackgroundColor,
        string NavbarBackgroundColor,
        string LogoCloudListStyleType,
        string TestimonialBackgroundColor,
        string CallToActionBackgroundColor,
        string FeatureGridDisplay,
        string HeroHeadingFontSize,
        string PricingTableBorderCollapse,
        string FaqAccordionTriggerFont);

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
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount === 0");
    }

    [Fact]
    public async Task CoreKineticCatalog_PausesCanvasBackgroundsOffscreenAndPreservesSemantics()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/core-kinetic-catalog");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 6");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=scramble-text-example]')?.textContent === 'Decoded on arrival.'");
        await page.WaitForFunctionAsync("() => document.querySelector('[data-testid=number-ticker-example] .syntax-circus-fancy-number-ticker__display')?.textContent === '1,284'");

        var rays = page.Locator("[data-testid='light-rays-example']");
        var meteors = page.Locator("[data-testid='meteor-example']");
        var grid = page.Locator("[data-testid='flicker-grid-example']");
        (await rays.Locator("canvas").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await meteors.Locator("canvas").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await grid.Locator("canvas").GetAttributeAsync("aria-hidden")).ShouldBe("true");
        (await page.Locator("[data-testid='number-ticker-example'] .syntax-circus-fancy-number-ticker__sr-only").InnerTextAsync()).ShouldBe("1,284");
        (await page.Locator("[data-testid='marquee-example'] .syntax-circus-fancy-marquee__content[aria-hidden='true']").GetAttributeAsync("inert")).ShouldNotBeNull();

        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount >= 3");
        var before = await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount");
        await rays.EvaluateAsync("element => element.style.transform = 'translateY(3000px)'");
        await meteors.EvaluateAsync("element => element.style.transform = 'translateY(3000px)'");
        await grid.EvaluateAsync("element => element.style.transform = 'translateY(3000px)'");
        await page.WaitForFunctionAsync($"() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount === {before - 3}");
    }

    [Fact]
    public async Task CoreKineticCatalog_WithReducedMotion_UsesStaticFallbacksAndFinalValues()
    {
        await using var context = await fixture.Browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/core-kinetic-catalog");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 6");

        (await page.Locator("[data-testid='light-rays-example'] canvas").EvaluateAsync<string>("element => getComputedStyle(element).display")).ShouldBe("none");
        (await page.Locator("[data-testid='meteor-example'] canvas").EvaluateAsync<string>("element => getComputedStyle(element).display")).ShouldBe("none");
        (await page.Locator("[data-testid='flicker-grid-example'] canvas").EvaluateAsync<string>("element => getComputedStyle(element).display")).ShouldBe("none");
        (await page.Locator("[data-testid='scramble-text-example']").InnerTextAsync()).ShouldBe("Decoded on arrival.");
        (await page.Locator("[data-testid='number-ticker-example'] .syntax-circus-fancy-number-ticker__sr-only").InnerTextAsync()).ShouldBe("1,284");
        (await page.Locator("[data-testid='marquee-example'] .syntax-circus-fancy-marquee__track").EvaluateAsync<string>("element => getComputedStyle(element).animationName")).ShouldBe("none");
        (await page.EvaluateAsync<int>("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics().animationFrameCount")).ShouldBe(0);
    }

    [Fact]
    public async Task CoreKineticCatalog_EnhancedNavigation_ReleasesAllSixEffects()
    {
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(fixture.TestHostUrl);
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 4");

        await page.Locator("header a[href='/core-kinetic-catalog']").ClickAsync();
        await page.WaitForURLAsync("**/core-kinetic-catalog");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 6");

        await page.Locator("header a[href='/border']").ClickAsync();
        await page.WaitForURLAsync("**/border");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 0");

        await page.Locator("a.wordmark").ClickAsync();
        await page.WaitForURLAsync(fixture.TestHostUrl + "/");
        await page.Locator("header a[href='/core-kinetic-catalog']").ClickAsync();
        await page.WaitForURLAsync("**/core-kinetic-catalog");
        await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount === 6");
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
        (await page.Locator("[data-testid='editorial-hero-example'] strong").InnerTextAsync()).ShouldBe("Editorial headline");
    }

    [Fact]
    public async Task KineticTextShowcase_RendersCyclesAndIsLinkedFromFourPlaces()
    {
        using var client = new HttpClient();
        var html = await client.GetStringAsync($"{fixture.TestHostUrl}/kinetic-text", TestContext.Current.CancellationToken);
        html.ShouldContain("syntax-circus-fancy-word-rotate");
        html.ShouldContain("syntax-circus-fancy-morph-text");
        html.ShouldContain("syntax-circus-fancy-typewriter");
        html.ShouldNotContain("data-fancy-state=\"out\"");

        var home = await client.GetStringAsync($"{fixture.TestHostUrl}/", TestContext.Current.CancellationToken);
        var occurrences = System.Text.RegularExpressions.Regex.Count(home, "href=\"/kinetic-text\"");
        occurrences.ShouldBeGreaterThanOrEqualTo(3);

        await using var browser = await NewWebGlBrowserAsync();
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync($"{fixture.TestHostUrl}/kinetic-text");
        await page.WaitForTimeoutAsync(200);

        (await page.Locator(".syntax-circus-fancy-word-rotate").Nth(0).GetAttributeAsync("data-fancy-disabled")).ShouldBe("false");
        (await page.Locator(".syntax-circus-fancy-morph-text").Nth(0).GetAttributeAsync("data-fancy-disabled")).ShouldBe("false");
        (await page.Locator(".syntax-circus-fancy-typewriter").Nth(0).GetAttributeAsync("data-fancy-disabled")).ShouldBe("false");

        await page.Locator("[data-testid='kinetic-lifecycle-toggle']").ClickAsync();
        await page.WaitForTimeoutAsync(150);
        var hostHtml = await page.Locator("[data-testid='kinetic-lifecycle-host']").InnerHTMLAsync();
        hostHtml.ShouldNotContain("syntax-circus-fancy-word-rotate");
        hostHtml.ShouldNotContain("syntax-circus-fancy-typewriter");
        (await page.Locator("[data-testid='kinetic-lifecycle-state']").InnerTextAsync()).ShouldBe("removed");

        var diagnostics = await page.EvaluateAsync<object>("() => globalThis.__syntaxCircusFancyBlazor.getDiagnostics()");
        diagnostics.ShouldNotBeNull();
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

        foreach (var route in new[] { "/background", "/border", "/reveal", "/tilt", "/spatial-surfaces", "/css-first-catalog", "/composition-authoring", "/threeui-inspiration", "/core-kinetic-catalog" })
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
            await using var browser = await NewWebGlBrowserAsync();
            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await using var webGlCleanup = new WebGlPageCleanup(page);
            await page.GotoAsync(standaloneUrl);
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazor?.instanceCount >= 3");
            await page.Locator("[data-testid='standalone-holographic']").ScrollIntoViewIfNeededAsync();
            await page.WaitForFunctionAsync("() => globalThis.__syntaxCircusFancyBlazorWebGl?.getDiagnostics().instances.some(instance => instance.active && instance.renderer)");

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

    private Task<IBrowser> NewWebGlBrowserAsync()
    {
        // WebGL context limits are browser-process scoped; isolate GPU-heavy tests
        // while continuing to share the compiled host and Playwright driver.
        return fixture.Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ["--use-angle=swiftshader", "--enable-unsafe-swiftshader"],
        });
    }

    private sealed class WebGlPageCleanup(IPage page) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            if (page.IsClosed)
            {
                return;
            }

            try
            {
                await page.GotoAsync("about:blank");
            }
            catch (PlaywrightException)
            {
                // Cleanup must not mask the test result if the browser already closed the page.
            }
        }
    }
}

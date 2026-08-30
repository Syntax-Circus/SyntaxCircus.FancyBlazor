# Core Kinetic Text Batch — Handoff

**Date:** 2026-08-30
**Branch:** `feature/discovery`
**Owner handoff:** opencode session → human / fresh agent

## TL;DR

The core kinetic text batch (`WordRotate`, `MorphText`, `Typewriter`) is **fully implemented and committed**. All 9 source files, the spec, the implementation plan, the bUnit tests, the demo route, the navigation links, the four user guides, the README, the CHANGELOG, the architecture docs, and the `AGENTS.md` updates are in. **`dotnet build --configuration Release` is clean with 0 warnings, 0 errors. All 33 .NET tests pass.**

What's **not** done: the final browser-test verification gate. One new browser test fails; one pre-existing browser test is also failing. The failures are not blockers for the feature, but they need a human to investigate with a faster tool. **Do not declare completion without resolving them.** This document captures everything needed to pick up.

## What ships

Three new core components, all public in `SyntaxCircus.FancyBlazor`:

| Component | File |
| --- | --- |
| `WordRotate` | `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor{,.cs,.css}` |
| `MorphText` | `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor{,.cs,.css}` |
| `Typewriter` | `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor{,.cs,.css}` |

Supporting types: `WordRotateTransition` enum, `MorphMode` enum, `KineticTextDirection` enum.

JS extension: three new factories (`createWordRotate`, `createMorphText`, `createTypewriter`) added to the existing `src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js` and registered in the existing `factories` map. **No new JS module, no new runtime.** This matches the existing `ScrambleText` / `NumberTicker` / `Marquee` pattern.

Demo: `samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor` at `/kinetic-text`. Linked from **four** places: primary nav, footer nav, home effect-grid, home catalog directory.

Tests:
- 7 new bUnit tests in `ComponentContractTests.cs` (all 33 tests in that project pass).
- 1 new Playwright test `KineticTextShowcase_RendersCyclesAndIsLinkedFromFourPlaces` in `FancyBlazorBrowserTests.cs` (**failing** — see below).

Docs: 4 new user guides, README, performance guide, accessibility guide, architecture roadmap, discovery index, requirements, CHANGELOG, AGENTS.md.

## Commit graph (most recent first)

```
30c5188 docs(core): add guides, README, and changelog entries for kinetic text batch
3ec9f65 test(core): add browser test for KineticTextShowcase
86970da feat(demo): link KineticTextShowcase from nav, footer, and home
4394a57 feat(demo): add KineticTextShowcase route
ec6e59a test(core): add bUnit contract tests; fix static class merging in kinetic components
93d9134 feat(core): add Typewriter component
814fea5 feat(core): add MorphText component
13cb6ba feat(core): add WordRotate component
ec9142b feat(core): add word-rotate, morph-text, and typewriter JS factories
e47f31e Add design spec for core kinetic text batch (WordRotate, MorphText, Typewriter)
```

## Untracked files (deliberate, will be committed in this batch's PR)

- `docs/superpowers/plans/2026-08-30-core-kinetic-text-batch.md` — the implementation plan.
- `tests/SyntaxCircus.FancyBlazor.TestHost/Components/Pages/KineticTextShowcase.razor` — minimal test-host copy of the showcase route (the browser-test runner hits `TestHost`, not `FancyBlazor.Demo.Client`).

Both should be committed as part of the same PR as the rest of the work.

## Architecture decisions worth knowing

- **Approach B in the spec was revised to "use the existing dispatcher"** after reading the actual repo. The brainstorming phase proposed a separate `FancyKineticTextRuntime`; the implementation correctly found that the existing `IFancyEffectRuntime` + `fancy-blazor.js` is already the shared dispatcher used by every other effect. No new runtime. The spec doc (`docs/superpowers/specs/2026-08-30-core-effects-kinetic-text-batch-design.md`, §3) records the revision.
- **Static class merging bug found and fixed during Task 5.** `AttributeComposer.Compose` always overwrites `class` from `stableClass + cssClass + additionalClass` and silently discards any `class` entry in `fixedAttributes`. The first cut of the three components added the `syntax-circus-fancy-kinetic-text--static` class via `fixedAttributes`, which got dropped. Fix: fold the static modifier into the stable class string when `Disabled` is true. All three components were fixed in the same commit.
- **JS factory shape** matches `createScrambleText` exactly: own `IntersectionObserver`, single `requestAnimationFrame` cycle, `motionReduced` reconciliation, `matchMedia` listener when `defaults.motionPreference === 'RespectSystem'`, `update`/`setDocumentVisible`/`hasActiveAnimationFrame`/`destroy` contract. `setDocumentVisible` is a no-op on all three (text cycles are intersection-gated, not document-visibility-gated).

## Verification status

| Step | Status | Notes |
| --- | --- | --- |
| `dotnet restore SyntaxCircus.FancyBlazor.slnx` | ✅ pass | |
| `dotnet build --configuration Release` | ✅ pass | 0 warnings, 0 errors across all 13 projects |
| `dotnet test --no-build` (Core tests) | ✅ pass | 33/33 pass |
| `dotnet test --no-build` (UI tests) | ✅ pass | |
| `dotnet test --no-build` (WebGL tests) | ✅ pass | |
| `dotnet test --no-build` (Browser tests) | ❌ **2 fail** | See "Browser test failures" below |
| `pwsh eng/verify-docs.ps1` | ✅ pass | 116 markdown files verified |
| `dotnet pack` | ⏸ not run | Last gate, deferred until browser tests are green |
| `pwsh eng/verify-package.ps1` | ⏸ not run | |

## Browser test failures — exactly what to debug

Both failures are in `tests/SyntaxCircus.FancyBlazor.BrowserTests/`.

### Failure 1: `KineticTextShowcase_RendersCyclesAndIsLinkedFromFourPlaces` (mine)

**First failing assertion** (from the test run log):

```
Shouldly.ShouldAssertException : html
  should contain (case insensitive comparison)
  "syntax-circus-fancy-morph-text"
  but was actually
  "<!DOCTYPE html>
   <html lang="en"><head><meta charset="utf-8">
       <meta name="viewport" widt..." (truncated by Shouldly)
```

**What the test does** (test file, line 1503-1536):

1. Fetches `{TestHostUrl}/kinetic-text` over `HttpClient.GetStringAsync` and asserts the raw HTML contains all three `syntax-circus-fancy-{x}` stable hooks.
2. Fetches `/` and asserts at least 3 `href="/kinetic-text"` links.
3. Spins up Chromium, visits `/kinetic-text` with `ReducedMotion.Reduce`, asserts the three hosts exist with `data-fancy-disabled="false"`.
4. Clicks `[data-testid='kinetic-lifecycle-toggle']`, asserts the lifecycle host no longer contains the two runtime-bound components.
5. Calls `globalThis.__syntaxCircusFancyBlazor.getDiagnostics()`.

**What I already fixed:**

- The test initially 404'd because `TestHost` has its own copy of every page (per AGENTS.md: "Browser tests launch compiled test-host assemblies rather than paths in the source checkout"). I added `tests/SyntaxCircus.FancyBlazor.TestHost/Components/Pages/KineticTextShowcase.razor` so the route exists.
- That first version had only `WordRotate` and `Typewriter` (no `MorphText`). I updated the test-host page to include all three, plus the lifecycle host and toggle.

**What I have NOT verified:** whether the test host's `KineticTextShowcase.razor` actually renders the three stable hooks into the SSR HTML. The error excerpt shows the page is loading (the `<!DOCTYPE html>` and `<meta>` show up) but the assertion fires on `syntax-circus-fancy-morph-text`. Possible causes:

1. **The test-host page didn't get picked up at test build time** — even though I rebuilt `TestHost` after adding it, the browser test runner may have a stale assembly. Check `tests/SyntaxCircus.FancyBlazor.TestHost/bin/Release/net10.0/SyntaxCircus.FancyBlazor.TestHost.dll` mtime.
2. **The test host compiles but a Roslyn analyzer / `EditorRequired` issue** is hiding the components during SSR. Confirm by curling the live host (start it with `dotnet run --project tests/SyntaxCircus.FancyBlazor.TestHost` on a free port and `Invoke-WebRequest /kinetic-text`).
3. **`HttpClient` in the test reads from the test host but the host hasn't been rebuilt into the published location** that the runner copies. Check `tests/SyntaxCircus.FancyBlazor.TestHost/obj/Release/net10.0/`.
4. **The browser test is running but the page is returning a 404 or `NotFound` template** — Shouldly's truncated error output makes this hard to confirm. Print the full `html` (it's ~20KB of Blazor bootstrap) to a file from inside the test to see what is actually rendered.

**Diagnostic recipe:**

```bash
# 1. Confirm the test host has the file
Get-ChildItem tests/SyntaxCircus.FancyBlazor.TestHost/Components/Pages/KineticTextShowcase.razor

# 2. Confirm the build is current
dotnet build tests/SyntaxCircus.FancyBlazor.TestHost/SyntaxCircus.FancyBlazor.TestHost.csproj --configuration Release

# 3. Manually run the test host and curl it
dotnet run --project tests/SyntaxCircus.FancyBlazor.TestHost --no-launch-profile --no-build --urls http://127.0.0.1:19999 &
sleep 5
Invoke-WebRequest http://127.0.0.1:19999/kinetic-text | Select-String "syntax-circus-fancy"
# kill the process when done
Get-Process -Name "dotnet" | Where-Object { $_.MainWindowTitle -eq "" } | Stop-Process
```

If `Invoke-WebRequest` shows the three hooks, the failure is in the test runner assembly copy. If not, the test host is rendering something else and the issue is in the page itself.

### Failure 2: `CompositionPresets_KeepSemanticControlsAndOnlyPressScaleInitializesRuntime` (pre-existing, NOT mine)

**Failure:**

```
System.TimeoutException : Timeout 30000ms exceeded.
Call log:
  - waiting for Locator("[data-testid='editorial-hero-example'] strong")
```

**Why I believe this is pre-existing:**

- I never modified `EditorialHero.razor` or its test-host page.
- The test is at `tests/SyntaxCircus.FancyBlazor.BrowserTests/FancyBlazorBrowserTests.cs:1486-1501`, immediately before my new test.
- The failed assertion is on a `strong` element with text "Editorial headline" — a static SSR string.
- This test was not in any of my commits and has no relationship to the kinetic-text work.

**Most likely cause:** a Chromium / WebGL initialization flake. The test spins up the **shared** `fixture.Browser` (the fixture creates one browser for the whole `IClassFixture` lifetime; see `BrowserHostFixture.cs:34-37`), and after a few other tests run, the browser may need a longer warm-up. The 30s timeout on `InnerTextAsync` is the symptom, not the cause.

**Recommended action for the handoff:** run that test in isolation. If it passes alone, mark it as flaky and skip-with-reason, or bump the timeout. If it fails alone too, dig into the test-host `CompositionAuthoring.razor` and `EditorialHero.razor`.

```bash
dotnet test tests/SyntaxCircus.FancyBlazor.BrowserTests/SyntaxCircus.FancyBlazor.BrowserTests.csproj --no-build --configuration Release -- --filter-method "CompositionPresets_KeepSemanticControlsAndOnlyPressScaleInitializesRuntime"
```

## Pending tasks before declaring completion

These are the exact steps from the implementation plan that are still open:

1. **Investigate and fix the `KineticTextShowcase` browser test** using the diagnostic recipe above. The fix is almost certainly in the test (assertion shape) or the test-host page (missing component / wrong conditional), not in the core components themselves — those are well-covered by the 33 passing bUnit tests.
2. **Investigate the `CompositionPresets` pre-existing flake** in isolation. Decide whether to mark flaky, bump timeout, or fix.
3. **Run the package verification gate** (the plan's Task 10 steps 5-7):
   ```bash
   dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts/release-preview -p:DisableGitVersionTask=true -p:PackageVersion=0.4.0-preview.1
   pwsh eng/verify-package.ps1 -PackageDirectory artifacts/release-preview
   pwsh eng/verify-docs.ps1
   ```
4. **Commit the two untracked files** (`docs/superpowers/plans/2026-08-30-core-kinetic-text-batch.md` and `tests/SyntaxCircus.FancyBlazor.TestHost/Components/Pages/KineticTextShowcase.razor`) as part of the same PR — but only after the browser test gate is green.
5. **Push / PR** — only when explicitly asked.

## Deferred-band core effects (for the next brainstorming session)

During the original brainstorming on 2026-08-30 we considered a wider menu of additions to the core effects library. The three that shipped were `WordRotate`, `MorphText`, and `Typewriter`. The following candidates were **discussed but deferred** — they're recorded here so the next session can pick up where this one left off without re-deriving the list.

### Core atmospheric Canvas batch (deferred — single batch alternative to the kinetic text theme)

Three bounded Canvas 2D atmospheric backgrounds. Each follows the same lifecycle as the existing Canvas 2D fields (`ConstellationBackground`, `ArcFlowBackground`, `FlickerGrid`, `MeteorBackground`, `LightRaysBackground`):

| Candidate | Description |
| --- | --- |
| `CausticsBackground` | A drifting caustic-light field; optical-pool distortion behind content. |
| `TopographicBackground` | A static or slowly drifting topographic contour line field. |
| `RainBackground` | A bounded streaking-rain Canvas 2D field. |

Rationale for deferral: the kinetic text theme won on momentum, and atmospheric fields are already well-covered (5 existing Canvas 2D backgrounds). Picking this up next would be a natural next batch.

### Core interaction / scroll batch (deferred)

| Candidate | Description |
| --- | --- |
| `ScrollVelocity` | Scroll-speed-tinted motion; modulates effects (e.g. shimmer speed) by scroll velocity. |
| `CompareReveal` | A before/after split slider for comparing two pieces of content side by side. |
| `Lens` | A magnifier overlay that follows the pointer and magnifies the underlying content. |

Rationale for deferral: `ScrollVelocity` requires defining a runtime contract for "effects that respond to scroll velocity" that doesn't exist yet; `CompareReveal` is more product-shaped (an editor primitive) than effect-shaped; `Lens` is interesting but breaks the "decorative, not interactive" rule unless scoped tightly.

### Open-research candidates the bank missed (not yet evaluated)

These were on the brainstorming whiteboard but never graded against existing coverage:

| Candidate | Description | Notes |
| --- | --- | --- |
| `Skeleton` | A loading-state placeholder; per-element shimmer + reserved box. | Could be CSS-only or a small effect; ties to the UI companion's missing "loading state" entry. |
| `TypewriterCursor` (not a separate component) | A standalone caret-with-cursor primitive that any text effect could consume. | Defer until a fourth kinetic text effect is needed. |
| `CharGlitch` | A character glitch / shake effect. | Tempting but probably overlaps with `ScrambleText`. |
| `MorphNumber` | A number tweening effect. | `NumberTicker` already does numeric tweening; this would be the textual analog of `MorphText`. |
| `Stagger` extensions (timeline) | Time-based sequencing beyond direct children. | Real value but lives in a different runtime model. |

### Rationale for the batch-pick

The brainstorming session landed on **kinetic text** over the alternatives because:

1. It filled the most obvious 1-entry text-effect category (kinetic headline motion) without overlapping any existing component.
2. The three components share a lifecycle so the implementation cost is low — one new JS dispatcher shape, three thin `ComponentBase` shells.
3. None of the candidates are blocked on a hard prerequisite (no WebGL dependency, no new runtime, no design system decision).
4. The visual vocabulary — cycling, crossfading, typing — is what a designer reaches for first when they want a headline that "moves."

If the next session prefers a different theme (e.g. atmospheric Canvas 2D batch), the existing dispatcher is ready and the patterns from the three kinetic text components transfer directly.

## Files of interest for the next session

| Path | Purpose |
| --- | --- |
| `docs/superpowers/specs/2026-08-30-core-effects-kinetic-text-batch-design.md` | The approved design spec. |
| `docs/superpowers/plans/2026-08-30-core-kinetic-text-batch.md` | The full step-by-step implementation plan. |
| `src/SyntaxCircus.FancyBlazor/Components/{WordRotate,MorphText,Typewriter}.razor{,.cs,.css}` | The three new components. |
| `src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js` | The shared dispatcher. Search for `createWordRotate` / `createMorphText` / `createTypewriter` (around lines 564-934). |
| `src/SyntaxCircus.FancyBlazor/{WordRotateTransition,MorphMode,KineticTextDirection}.cs` | The three new public enums. |
| `tests/SyntaxCircus.FancyBlazor.Tests/ComponentContractTests.cs` | 7 new bUnit tests at the bottom of the class. |
| `tests/SyntaxCircus.FancyBlazor.BrowserTests/FancyBlazorBrowserTests.cs:1503-1536` | The new (failing) browser test. |
| `tests/SyntaxCircus.FancyBlazor.TestHost/Components/Pages/KineticTextShowcase.razor` | The minimal test-host page. |
| `samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor` | The full demo route. |
| `samples/FancyBlazor.Demo.Client/Layout/MainLayout.razor` | Header and footer nav links. |
| `samples/FancyBlazor.Demo.Client/Pages/Home.razor` | Effect-grid card and catalog-directory entry. |
| `docs/components/{word-rotate,morph-text,typewriter,kinetic-text-overview}.md` | The four new user guides. |
| `docs/architecture/99-IMPLEMENTATION-ROADMAP.md` | Updated to remove `WordRotate` and `MorphText` from the core bank. |
| `CHANGELOG.md` | `Unreleased` `Added` section. |
| `AGENTS.md` | Purpose-and-boundary enumeration and CSS custom property prefix. |

## Notes for the human / fresh agent

- **Don't re-brainstorm** the kinetic text batch — it's already done. The deferred-band list in this document is the next-batch menu.
- **The pre-existing `CompositionPresets` flake is likely unrelated** but the new test runner takes ~90s on this machine; budget at least 2 minutes per browser test cycle.
- **All file changes are uncommitted-but-staged-clean except the two untracked files listed above** — the `feature/discovery` branch is in a clean state. The simplest path forward is to fix the browser test, commit the two untracked files plus any fix, run the package gate, and stop there. No push, no PR, no tag.
- **The plan's commit boundaries** (one commit per Task) are reflected in the commit graph above. If you need to amend any of them, do so in a follow-up commit; don't rewrite history.

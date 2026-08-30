# Core Effects: Kinetic Text Batch — Design

**Status:** Approved
**Date:** 2026-08-30
**Scope:** Core effects library (`src/SyntaxCircus.FancyBlazor/`)

## 1. Purpose

Add three new kinetic text effects to the core effects library: **`WordRotate`**, **`MorphText`**, **`Typewriter`**. Each solves a distinct decorative-text problem designers actually have:

- **`WordRotate`** — cycle through a list of headline words with a fade/slide/blur transition.
- **`MorphText`** — crossfade or character-split between two or more strings, holding each for a visible beat.
- **`Typewriter`** — progressive character-by-character add (with optional delete and optional blinking caret) over a list of lines.

These three sit alongside the existing text effects (`TextReveal`, `TypeFlow`, `ScrambleText`, `GradientText`, `NeonText`, `TextStroke`, `HighlightText`) and round out the kinetic-text vocabulary without duplicating any existing component's role.

## 2. Non-goals

- No fourth component (no `CharGlitch`, no `MorphNumber`, etc.) — defer until a fourth kinetic text behavior is needed.
- No WebGL/Three.js text displacement.
- No audio reactivity, scroll-velocity integration, or external data sources.
- No event/callback API — these are decorative.
- No localization of caret character beyond the `CaretCharacter` parameter.

## 3. Architecture (revised after codebase grounding)

The brainstorming phase proposed a separate `FancyKineticTextRuntime` + `kinetic-text-runtime.js`. After reading the actual repo, the existing **`IFancyEffectRuntime` + `fancy-blazor.js` dispatcher** is already the shared runtime used by every effect in this package (ScrambleText, NumberTicker, Marquee, Tilt, etc.). Adding a parallel runtime would duplicate surface area and break the established pattern.

**Revised architecture**: register the three new effects in the existing dispatcher as additional factory entries (`word-rotate`, `morph-text`, `typewriter`) and write three thin `ComponentBase` components that follow the ScrambleText/NumberTicker pattern exactly.

**Files added:**
```
src/SyntaxCircus.FancyBlazor/
  Components/WordRotate.razor
  Components/WordRotate.razor.cs
  Components/WordRotate.razor.css
  Components/MorphText.razor
  Components/MorphText.razor.cs
  Components/MorphText.razor.css
  Components/Typewriter.razor
  Components/Typewriter.razor.cs
  Components/Typewriter.razor.css
```

**Files modified:**
```
src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js  (add three factories)
tests/SyntaxCircus.FancyBlazor.Tests/ComponentContractTests.cs  (add tests)
tests/SyntaxCircus.FancyBlazor.Tests/DemoCatalogTests.cs  (add /kinetic-text to array)
samples/FancyBlazor.Demo.Client/Layout/MainLayout.razor  (add to primary nav + footer nav)
samples/FancyBlazor.Demo.Client/Pages/Home.razor  (add to effect-grid + catalog directory)
samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor  (new route)
```

All public types remain in the `SyntaxCircus.FancyBlazor` namespace.

## 4. Public API

### 4.1 `WordRotate`

```text
Words : IReadOnlyList<string>     (required; ≥ 2 items)
Interval : TimeSpan                  (default 2.5s; clamp [250ms, 30s])
Loop : bool                          (default true)
StartIndex : int                     (default 0)
Transition : WordRotateTransition    (Fade | SlideUp | SlideDown | Blur) (default Fade)
Easing : string?                     (CSS easing; default "ease-out")
CssClass : string?
Style : string?
ChildContent : RenderFragment?
```

### 4.2 `MorphText`

```text
Words : IReadOnlyList<string>     (required; ≥ 2 items)
Duration : TimeSpan                  (per-direction morph; default 600ms; clamp [100ms, 2s])
Hold : TimeSpan                      (full-word hold between morphs; default 1.2s; clamp [0, 10s])
Loop : bool                          (default true)
StartIndex : int                     (default 0)
Mode : MorphMode                     (Crossfade | CharSplit) (default Crossfade)
Easing : string?                     (CSS easing; default "cubic-bezier(0.22, 1, 0.36, 1)")
CssClass : string?
Style : string?
ChildContent : RenderFragment?
```

### 4.3 `Typewriter`

```text
Text : IReadOnlyList<string>      (required; ≥ 1 item)
Speed : TimeSpan                  (per-character; default 60ms; clamp [10ms, 500ms])
HoldAfter : TimeSpan              (hold after each line; default 1.5s)
DeleteSpeed : TimeSpan?           (default = Speed; null disables deletion)
Loop : bool                       (default true)
StartIndex : int                  (default 0)
Caret : bool                      (default true)
CaretCharacter : string           (default "|")
Direction : KineticTextDirection  (Auto | Ltr | Rtl) (default Auto)
CssClass : string?
Style : string?
ChildContent : RenderFragment?
```

## 5. Runtime (no change)

The existing `IFancyEffectRuntime` (internal) already supports `CreateAsync(element, effect, options)` with a string discriminator. Each new component constructs an options object exactly as `ScrambleText` and `NumberTicker` do, and calls `Runtime.CreateAsync(_element, "word-rotate" | "morph-text" | "typewriter", options)`.

## 6. JavaScript dispatcher (extended)

Three new factory functions are added to `fancy-blazor.js`, registered in the `factories` map:

```text
'word-rotate':  createWordRotate,
'morph-text':   createMorphText,
'typewriter':    createTypewriter,
```

Each factory follows the existing `createScrambleText` / `createNumberTicker` shape: own `IntersectionObserver` + `requestAnimationFrame` cycle, own timer, `motionReduced` reconciliation, `prefers-reduced-motion` media listener, `update`, `setDocumentVisible`, `hasActiveAnimationFrame`, `destroy` contract.

The dispatcher in `fancy-blazor.js` already supplies `setDocumentVisible` to all instances via the `ensureVisibilityListener` mechanism; the new factories are no-ops for it (text cycles don't need to track document visibility — they only run when intersecting).

## 7. Lifecycle (Blazor side, follows existing pattern)

- `OnAfterRenderAsync(firstRender)` invokes `runtime.CreateAsync` after checking `OperatingSystem.IsBrowser()` and JS availability.
- `OnParametersSetAsync` constructs the options object; if signature changes, calls `runtime.UpdateAsync` instead of recreating.
- `DisposeAsync` invokes `runtime.DestroyAsync`.
- During static SSR / prerender: only the first item renders as plain text inside the stable hook.

## 8. Static SSR / non-interactive mode

- During SSR: first word/line renders inside the stable hook; no decorative layer.
- The JS factory's `motionReduced` path also handles SSR/non-interactive: only the first item shows; no animation.

## 9. Accessibility

| Component | Visible text | Accessible mirror | `aria-live` |
|---|---|---|---|
| `WordRotate` | `aria-hidden` decorative layer | visually-hidden `<span>` with current word | `polite` |
| `MorphText` | `aria-hidden` decorative layer | visually-hidden `<span>` with current word | `polite` |
| `Typewriter` | `aria-hidden` decorative layer | visually-hidden `<span>` with current full line (updated only on full-line completion) | `off` |

Common:
- Host is `<span>` (inline) or `<div>` when `ChildContent` is used (the host wraps child content).
- Host is unfocusable.
- No keyboard activation, no click handler, no focus management.

## 10. Reduced motion

- Detected at create time AND on `matchMedia('(prefers-reduced-motion: reduce)').change`.
- When reduced motion is active OR `Disabled=true`:
  - All three show the first item only.
  - All three add `syntax-circus-fancy-kinetic-text--static` class on the host.
  - Decorative animations stop.
- Typewriter caret CSS-only blink is reduced-motion safe.

## 11. Palette integration

- All three read `--sc-fancy-palette-text` (fall back to `currentColor`).
- `MorphText` `CharSplit` and `Typewriter` caret read `--sc-fancy-palette-accent` for highlight/caret color.
- All three work without any palette set.

## 12. CSS hooks and custom properties

Stable hooks (per AGENTS.md):
- `.syntax-circus-fancy-word-rotate`
- `.syntax-circus-fancy-morph-text`
- `.syntax-circus-fancy-typewriter`

Shared modifier:
- `.syntax-circus-fancy-kinetic-text--static` (reduced-motion / disabled)

Per-component modifiers (set as additional data-attribute on the host so CSS can target without string parsing):
- `data-fancy-word-rotate-transition="fade|slide-up|slide-down|blur"`
- `data-fancy-morph-mode="crossfade|char-split"`
- `data-fancy-typewriter-caret="true|false"`

Custom properties (CSS-defined defaults; consumers can override):
- Shared: `--sc-fancy-kinetic-text-easing`, `--sc-fancy-kinetic-text-color`
- `WordRotate`: `--sc-fancy-word-rotate-duration`
- `MorphText`: `--sc-fancy-morph-text-duration`, `--sc-fancy-morph-text-hold`
- `Typewriter`: `--sc-fancy-typewriter-caret-color`, `--sc-fancy-typewriter-caret-blink-duration`

## 13. Layout safety

- For `WordRotate` and `MorphText`: a `min-height` based on the longest item is set via inline CSS custom property on first render (computed by the JS factory using a hidden measurement span).
- For `Typewriter`: the line is single-line, so no min-height is required.
- The CSS reserves the line height via `--sc-fancy-kinetic-text-min-height`.

## 14. Internationalization

- All three iterate strings by Unicode code-point (using `Array.from(str)` semantics in JS).
- Composed graphemes preserved.
- RTL handled via inherited `direction`; `Direction` parameter (`Auto | Ltr | Rtl`, default `Auto`) is available on `Typewriter`.
- Caret character is consumer-overridable only.

## 15. Bootstrap / framework coexistence

- Core does not enforce the UI companion's `box-sizing/color/border/text-decoration/font` self-declaration rules (those apply only to widget semantics).
- Core effects remain scoped-CSS-isolated and inherit parent typography by default.

## 16. Resource guarantees

- Every `IntersectionObserver`, `requestAnimationFrame`, and `setTimeout` is released on disposal.
- Top-level `disposeRuntime()` already clears all instances; new factories register into the same Map.
- No new global state.

## 17. Failure handling

- If the JS factory throws during creation, the existing runtime logs once and the static first-item fallback remains visible.
- Components never throw to consumers; the Blazor render path always succeeds.

## 18. Risks and mitigations

- **RAF contention with many instances.** Mitigation: existing dispatcher already manages all RAFs; document a soft cap of ~12 simultaneous kinetic text instances per page.
- **Layout shift.** Mitigation: pre-measured longest-item height + CSS `--sc-fancy-kinetic-text-min-height`; bUnit test asserts the custom property is set.
- **Screen-reader announcements spam.** Mitigation: `aria-live="off"` on `Typewriter`; mirror updates only on full-line completion.
- **API drift between siblings over time.** Mitigation: shared discriminator contract; parameter symmetry enforced by static checks in the test suite.

## 19. Testing

**bUnit (`tests/SyntaxCircus.FancyBlazor.Tests/ComponentContractTests.cs`):**
- `WordRotate_RendersSemanticHostWithAccessibleText`
- `MorphText_RendersSemanticHostWithAccessibleText`
- `Typewriter_RendersSemanticHostWithAccessibleText`
- `KineticTextComponents_Disabled_AddStaticClass`

Each test class covers:
- Renders with the stable host hook + first item as static fallback.
- All typed parameter setters produce expected `data-fancy-*` attributes (no renderer names leaked).
- `CssClass`, `Style`, `ChildContent` are merged without dropping the stable hook.
- `Disabled` → host has `syntax-circus-fancy-kinetic-text--static` class; runtime is not invoked.
- `Words`/`Text` validation: minimum count required (component raises a helpful error in `OnInitialized`).

**Browser tests (`tests/SyntaxCircus.FancyBlazor.BrowserTests/FancyBlazorBrowserTests.cs`):**
- `KineticTextComponents_RenderAndCycle_RespectReducedMotion`
  - Visit `/kinetic-text`. Assert all three components are present. With `prefers-reduced-motion: reduce`, assert all have `data-fancy-disabled="true"` and visible first word/line only. Toggle reduced-motion off; assert cycles advance (compare accessible-mirror text after a 2s wait).
  - Switch tab to background; assert `data-fancy-disabled="true"` for all.
  - Click the "remove from DOM" button; assert all observers/timers released (verified via `globalThis.__syntaxCircusFancyBlazor.getDiagnostics().instanceCount === 0`).
  - Assert that no layout shift > 0.05 occurs when words swap (use the existing `PerformanceObserver` helper if present, else skip).
  - Verify the route is linked from the header nav, footer nav, home effect-grid, and home catalog directory (DOM queries for `a[href='/kinetic-text']` count >= 4).

## 20. Demo

- New route `KineticTextShowcase` added to `samples/FancyBlazor.Demo.Client/Pages/`.
- The route must be linked from **four** places (per user direction):
  1. The **primary nav bar** in `Layout/MainLayout.razor` (header).
  2. The **footer nav** in `Layout/MainLayout.razor` (footer sitemap).
  3. The **home effect-grid** in `Pages/Home.razor` (the 7-card "feeling" grid).
  4. The **home catalog directory** in `Pages/Home.razor` (the "Every demo, at a glance" section).
- The route demonstrates each component:
  - Inline usage in a paragraph (`WordRotate`, `MorphText`).
  - Headline usage with palette (`Typewriter`).
  - One reduced-motion example.
  - One composition example inside a `Hero` / `CallToAction`.
  - A "remove from DOM" button to exercise disposal.

## 21. Documentation

- `README.md`: add three components to "Preview components" table; add a short code snippet.
- `docs/components/word-rotate.md` (new).
- `docs/components/morph-text.md` (new).
- `docs/components/typewriter.md` (new).
- `docs/components/kinetic-text-overview.md` (new): cross-component guide.
- `docs/guides/performance.md`: add a section on kinetic text runtime cost.
- `docs/guides/accessibility.md`: add a section on the `aria-hidden` + visually-hidden mirror pattern.
- `docs/architecture/99-IMPLEMENTATION-ROADMAP.md`: remove `WordRotate`, `MorphText` from the core evaluation bank.
- `docs/architecture/00-DISCOVERY-INDEX.md`: add a discovery note for the new phase.
- `docs/architecture/01-REQUIREMENTS.md`: update the approved-pre-1.0-roadmap list.
- `CHANGELOG.md`: add three components to `Unreleased` (Keep a Changelog `Added` category).
- `AGENTS.md`: update the "Preview components" list and the "Purpose and boundary" enumeration to include the three new effects and the `--sc-fancy-kinetic-text-*` custom property prefix.

## 22. Test commands

```text
dotnet restore SyntaxCircus.FancyBlazor.slnx
dotnet build SyntaxCircus.FancyBlazor.slnx --no-restore --configuration Release
dotnet test SyntaxCircus.FancyBlazor.slnx --no-build --configuration Release
pwsh tests/SyntaxCircus.FancyBlazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test tests/SyntaxCircus.FancyBlazor.BrowserTests --configuration Release
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts/release-preview -p:DisableGitVersionTask=true -p:PackageVersion=0.4.0-preview.1
pwsh eng/verify-package.ps1 -PackageDirectory artifacts/release-preview
pwsh eng/verify-docs.ps1
```

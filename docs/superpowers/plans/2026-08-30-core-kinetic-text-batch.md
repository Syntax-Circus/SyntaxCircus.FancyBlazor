# Core Kinetic Text Batch Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add three new kinetic text effects (`WordRotate`, `MorphText`, `Typewriter`) to the core FancyBlazor package, with shared JS dispatcher entry points in `fancy-blazor.js`, bUnit contract tests, browser tests, a compiling demo route linked from four locations, and full documentation/CHANGELOG/AGENTS.md updates.

**Architecture:** Three thin `ComponentBase` shells that follow the existing `ScrambleText` / `NumberTicker` / `Marquee` pattern. Each constructs an options object and calls the existing shared `IFancyEffectRuntime.CreateAsync(_element, "word-rotate" | "morph-text" | "typewriter", options)`. Three new factory functions are added to `wwwroot/js/fancy-blazor.js` and registered in the existing `factories` map. No new runtime, no new JS module.

**Tech Stack:** .NET 10, Blazor, Razor CSS isolation, bUnit, xUnit, Shouldly, Playwright, ES modules (vanilla).

**Spec:** `docs/superpowers/specs/2026-08-30-core-effects-kinetic-text-batch-design.md`

## Global Constraints

- **.NET 10**, C# latest, Blazor WASM + Interactive Auto.
- **No new public namespaces**; all new types in `SyntaxCircus.FancyBlazor`.
- **JS-light**: components send create/update/destroy; JS owns RAF, observer, timers. **No frame updates go to .NET.**
- **Stable CSS hook required** on every host: `syntax-circus-fancy-{word-rotate|morph-text|typewriter}`.
- **Visual layer `aria-hidden="true"`**; accessible text in a `visually-hidden` mirror.
- **`Disabled` →** host gets `syntax-circus-fancy-kinetic-text--static` class and runtime is destroyed.
- **`prefers-reduced-motion: reduce`** → JS factory shows first item only; no animation.
- **Decorative failures log once and retain the first-item fallback**; never throw to consumer.
- **All C# `TimeSpan` durations**; clamp unsafe numerics via `AttributeComposer.NonNegative` / `AttributeComposer.Clamp`.
- **CSS custom properties** named `--sc-fancy-*`.
- **Decorative elements** are `aria-hidden`, unfocusable, and pointer-transparent.
- **No new public services in DI** — the existing `IFancyEffectRuntime` is the only one.

---

## File Structure

### Created files

| File | Responsibility |
|---|---|
| `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor` | Razor template: `<span>` host, decorative inner `<span class="syntax-circus-fancy-word-rotate__display">`, accessible mirror `<span class="visually-hidden">`. |
| `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor.cs` | C# code-behind: `IFancyEffectRuntime` create/update/destroy wiring, parameter validation, signature diff. |
| `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor.css` | Scoped styles for transitions and `--static` modifier. |
| `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor` | Razor template: same host shape as WordRotate, with two stacked inner spans for crossfade. |
| `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor.cs` | C# code-behind. |
| `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor.css` | Scoped styles for crossfade and char-split modes. |
| `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor` | Razor template: host with caret + decorative spans; accessible mirror. |
| `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor.cs` | C# code-behind. |
| `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor.css` | Scoped styles for caret blink and `--static` modifier. |
| `docs/components/word-rotate.md` | User guide. |
| `docs/components/morph-text.md` | User guide. |
| `docs/components/typewriter.md` | User guide. |
| `docs/components/kinetic-text-overview.md` | Cross-component guide. |
| `samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor` | Compiling demo route. |

### Modified files

| File | Change |
|---|---|
| `src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js` | Add `createWordRotate`, `createMorphText`, `createTypewriter` factories and register them in the `factories` map. |
| `tests/SyntaxCircus.FancyBlazor.Tests/ComponentContractTests.cs` | Add three new test methods + one combined test. |
| `tests/SyntaxCircus.FancyBlazor.Tests/DemoCatalogTests.cs` | Add `"/kinetic-text"` to `DemoDestinations` array at the correct position. |
| `samples/FancyBlazor.Demo.Client/Layout/MainLayout.razor` | Add `<NavLink href="/kinetic-text">` in header nav and footer nav. |
| `samples/FancyBlazor.Demo.Client/Pages/Home.razor` | Add a link in the effect-grid (position: after "Voice/Expressive effects" cell) and a link in the catalog directory under "Voice". |
| `tests/SyntaxCircus.FancyBlazor.BrowserTests/FancyBlazorBrowserTests.cs` | Add one browser test `KineticTextShowcase_RendersCyclesAndIsLinkedFromFourPlaces`. |
| `README.md` | Add three rows to the "Preview components" table. |
| `docs/guides/performance.md` | Add a short section on kinetic text runtime cost. |
| `docs/guides/accessibility.md` | Add a short section on the `aria-hidden` + visually-hidden mirror pattern. |
| `docs/architecture/99-IMPLEMENTATION-ROADMAP.md` | Remove `WordRotate`, `MorphText` from core evaluation bank. |
| `docs/architecture/00-DISCOVERY-INDEX.md` | Add a discovery note for the new phase. |
| `docs/architecture/01-REQUIREMENTS.md` | Update the approved-pre-1.0-roadmap list. |
| `CHANGELOG.md` | Add three components to `Unreleased` `Added` section. |
| `AGENTS.md` | Update the "Preview components" list and "Purpose and boundary" enumeration. |

---

## Task 1: Add three JS factories to the dispatcher

**Files:**
- Modify: `src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js`
- (no new files; no test files yet — JS changes are validated through the new bUnit tests + browser test in later tasks)

**Interfaces:**
- Consumes: existing `factories` map at line ~56 of `fancy-blazor.js`; existing helpers `motionReduced(defaults.motionPreference, media)` (line ~22-25 of the same file); existing `setState(element, stateName)` helper.
- Produces: three new exported factory functions `createWordRotate`, `createMorphText`, `createTypewriter` matching the contract `{ update, setDocumentVisible, hasActiveAnimationFrame, destroy }`.

**Contract every factory must satisfy** (derived from existing `createScrambleText` / `createNumberTicker`):
- On create: read initial options, build the visual layer(s), set up `IntersectionObserver` (threshold 0.1), `requestAnimationFrame` cycle, `matchMedia('(prefers-reduced-motion: reduce)')` listener when `defaults.motionPreference === 'RespectSystem'`, `data-fancy-ready="true"` on host.
- On reduced motion: settle to first item; do not animate.
- On `update`: tear down prior state, reconfigure.
- On `destroy`: cancel all RAF, clear all timers, disconnect observer, remove media listener, clear decorative children, delete data attributes, leave the host with the first item as plain text.
- `setDocumentVisible`: no-op (text cycles only run when intersecting).
- `hasActiveAnimationFrame`: return whether the current `frame` handle is non-null.

- [ ] **Step 1: Add `createWordRotate` factory**

Insert after `createNumberTicker` (line ~618). The factory:

```js
function createWordRotate(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    let index = Math.max(0, Math.floor(Number(options.startIndex) || 0));
    let lastSwapAt = 0;
    let display = null;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const words = () => Array.isArray(options.words) ? options.words : [];
    const reduced = () => motionReduced(defaults.motionPreference, media);

    const ensureDisplay = () => {
        if (display) return display;
        const d = document.createElement('span');
        d.className = 'syntax-circus-fancy-word-rotate__display';
        d.setAttribute('aria-hidden', 'true');
        element.append(d);
        display = d;
        return d;
    };

    const settle = () => {
        const list = words();
        if (list.length === 0) return;
        const d = ensureDisplay();
        d.textContent = list[index % list.length] ?? '';
        element.setAttribute('aria-label', d.textContent);
        d.dataset.fancyState = 'idle';
    };

    const applyTransition = (next) => {
        const d = ensureDisplay();
        const transition = String(options.transition || 'fade');
        d.dataset.fancyTransition = transition;
        d.dataset.fancyState = 'out';
        const cleanup = () => {
            d.textContent = next;
            d.dataset.fancyState = 'in';
            element.setAttribute('aria-label', next);
            d.removeEventListener('transitionend', cleanup);
        };
        d.addEventListener('transitionend', cleanup, { once: true });
    };

    const tick = (now) => {
        if (destroyed) return;
        const list = words();
        if (list.length < 2) { frame = null; return; }
        const interval = Math.max(1, Number(options.interval) || 1);
        if (lastSwapAt === 0) lastSwapAt = now;
        if (now - lastSwapAt >= interval) {
            lastSwapAt = now;
            index = (index + 1) % list.length;
            applyTransition(list[index]);
        }
        frame = requestAnimationFrame(tick);
    };

    const configure = () => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) clearTimeout(timer);
        display?.remove();
        display = null;
        const list = words();
        if (list.length === 0) return;
        if (reduced() || list.length < 2) { settle(); return; }
        index = Math.min(index, list.length - 1);
        const initial = list[index];
        const d = ensureDisplay();
        d.textContent = initial;
        element.setAttribute('aria-label', initial);
        d.dataset.fancyTransition = String(options.transition || 'fade');
        d.dataset.fancyState = 'in';
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) { if (frame !== null) { cancelAnimationFrame(frame); frame = null; } }
                else if (frame === null) { lastSwapAt = 0; frame = requestAnimationFrame(tick); }
            }
        }, { threshold: 0.1 });
        observer.observe(element);
    };

    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();

    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            if (frame !== null) cancelAnimationFrame(frame);
            if (timer !== null) clearTimeout(timer);
            observer?.disconnect();
            media?.removeEventListener('change', mediaHandler);
            display?.remove();
            display = null;
            const list = words();
            element.textContent = list.length > 0 ? (list[0] ?? '') : '';
            element.removeAttribute('aria-label');
        },
    };
}
```

- [ ] **Step 2: Add `createMorphText` factory**

Insert after `createWordRotate`. The factory:

```js
function createMorphText(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    let index = Math.max(0, Math.floor(Number(options.startIndex) || 0));
    let phase = 'hold';
    let phaseStart = 0;
    let front = null;
    let back = null;
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const words = () => Array.isArray(options.words) ? options.words : [];
    const reduced = () => motionReduced(defaults.motionPreference, media);

    const ensureLayer = (cls) => {
        const layer = document.createElement('span');
        layer.className = cls;
        layer.setAttribute('aria-hidden', 'true');
        element.append(layer);
        return layer;
    };

    const settle = () => {
        const list = words();
        if (list.length === 0) return;
        front?.remove(); back?.remove();
        front = ensureLayer('syntax-circus-fancy-morph-text__layer');
        front.textContent = list[index % list.length] ?? '';
        element.setAttribute('aria-label', front.textContent);
        front.dataset.fancyState = 'in';
    };

    const tick = (now) => {
        if (destroyed) return;
        const list = words();
        if (list.length < 2) { frame = null; return; }
        const duration = Math.max(1, Number(options.duration) || 1);
        const hold = Math.max(0, Number(options.hold) || 0);
        if (phase === 'hold' && now - phaseStart >= hold) {
            const nextIndex = (index + 1) % list.length;
            const next = list[nextIndex];
            back.textContent = next;
            back.dataset.fancyState = 'in';
            front.dataset.fancyState = 'out';
            const onEnd = () => {
                front.textContent = next;
                front.dataset.fancyState = 'in';
                back.dataset.fancyState = 'idle';
                element.setAttribute('aria-label', next);
                index = nextIndex;
                phase = 'hold';
                phaseStart = performance.now();
                front.removeEventListener('transitionend', onEnd);
            };
            front.addEventListener('transitionend', onEnd, { once: true });
            phase = 'morph';
            phaseStart = now;
        } else if (phase === 'morph' && now - phaseStart >= duration) {
            phase = 'hold';
            phaseStart = now;
        }
        frame = requestAnimationFrame(tick);
    };

    const configure = () => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) clearTimeout(timer);
        front?.remove(); back?.remove();
        front = null; back = null;
        const list = words();
        if (list.length === 0) return;
        if (reduced() || list.length < 2) { settle(); return; }
        index = Math.min(index, list.length - 1);
        const initial = list[index];
        front = ensureLayer('syntax-circus-fancy-morph-text__layer');
        back = ensureLayer('syntax-circus-fancy-morph-text__layer');
        front.textContent = initial; front.dataset.fancyState = 'in';
        back.textContent = initial; back.dataset.fancyState = 'idle';
        element.setAttribute('aria-label', initial);
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) { if (frame !== null) { cancelAnimationFrame(frame); frame = null; } }
                else if (frame === null) { phase = 'hold'; phaseStart = performance.now(); frame = requestAnimationFrame(tick); }
            }
        }, { threshold: 0.1 });
        observer.observe(element);
    };

    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();

    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            if (frame !== null) cancelAnimationFrame(frame);
            if (timer !== null) clearTimeout(timer);
            observer?.disconnect();
            media?.removeEventListener('change', mediaHandler);
            front?.remove(); back?.remove();
            front = null; back = null;
            const list = words();
            element.textContent = list.length > 0 ? (list[0] ?? '') : '';
            element.removeAttribute('aria-label');
        },
    };
}
```

- [ ] **Step 3: Add `createTypewriter` factory**

Insert after `createMorphText`. The factory:

```js
function createTypewriter(element, initialOptions, defaults) {
    let options = initialOptions;
    let observer = null;
    let frame = null;
    let timer = null;
    let destroyed = false;
    let index = Math.max(0, Math.floor(Number(options.startIndex) || 0));
    let phase = 'typing';
    let phaseStart = 0;
    let charIndex = 0;
    let textEl = null;
    let caretEl = null;
    let visibleText = '';
    const media = defaults.motionPreference === 'RespectSystem' ? matchMedia('(prefers-reduced-motion: reduce)') : null;
    const lines = () => Array.isArray(options.text) ? options.text : [];
    const reduced = () => motionReduced(defaults.motionPreference, media);

    const ensureElements = () => {
        if (textEl) return;
        textEl = document.createElement('span');
        textEl.className = 'syntax-circus-fancy-typewriter__text';
        textEl.setAttribute('aria-hidden', 'true');
        element.append(textEl);
        if (options.caret !== false) {
            caretEl = document.createElement('span');
            caretEl.className = 'syntax-circus-fancy-typewriter__caret';
            caretEl.setAttribute('aria-hidden', 'true');
            element.append(caretEl);
        }
    };

    const syncAccessible = () => {
        const list = lines();
        if (list.length === 0) return;
        element.setAttribute('aria-label', list[index % list.length] ?? '');
    };

    const settle = () => {
        const list = lines();
        if (list.length === 0) return;
        ensureElements();
        visibleText = list[index % list.length] ?? '';
        textEl.textContent = visibleText;
        if (caretEl) caretEl.textContent = '';
        syncAccessible();
    };

    const tick = (now) => {
        if (destroyed) return;
        const list = lines();
        if (list.length === 0) { frame = null; return; }
        const speed = Math.max(1, Number(options.speed) || 1);
        const deleteSpeed = options.deleteSpeed == null ? speed : Math.max(1, Number(options.deleteSpeed) || speed);
        const holdAfter = Math.max(0, Number(options.holdAfter) || 0);
        const loop = options.loop !== false;
        const current = list[index % list.length] ?? '';
        const chars = Array.from(current);
        if (phase === 'typing') {
            if (charIndex < chars.length) {
                if (now - phaseStart >= speed) {
                    charIndex++;
                    visibleText = chars.slice(0, charIndex).join('');
                    textEl.textContent = visibleText;
                    phaseStart = now;
                }
            } else {
                phase = 'holdAfter';
                phaseStart = now;
            }
        } else if (phase === 'holdAfter') {
            if (now - phaseStart >= holdAfter) {
                if (charIndex > 0 && options.deleteSpeed !== null) { phase = 'deleting'; phaseStart = now; }
                else { advanceLine(list, loop); }
            }
        } else if (phase === 'deleting') {
            if (charIndex > 0) {
                if (now - phaseStart >= deleteSpeed) {
                    charIndex--;
                    visibleText = chars.slice(0, charIndex).join('');
                    textEl.textContent = visibleText;
                    phaseStart = now;
                }
            } else {
                advanceLine(list, loop);
            }
        }
        frame = requestAnimationFrame(tick);
    };

    const advanceLine = (list, loop) => {
        if (index + 1 >= list.length) {
            if (!loop) { frame = null; visibleText = list[index % list.length] ?? ''; textEl.textContent = visibleText; syncAccessible(); return; }
            index = 0;
        } else {
            index++;
        }
        syncAccessible();
        phase = 'typing';
        charIndex = 0;
        phaseStart = performance.now();
    };

    const configure = () => {
        observer?.disconnect(); observer = null;
        if (frame !== null) cancelAnimationFrame(frame);
        if (timer !== null) clearTimeout(timer);
        textEl?.remove(); caretEl?.remove();
        textEl = null; caretEl = null;
        const list = lines();
        if (list.length === 0) return;
        if (reduced()) { settle(); return; }
        index = Math.min(index, list.length - 1);
        ensureElements();
        visibleText = '';
        charIndex = 0;
        textEl.textContent = '';
        phase = 'typing';
        phaseStart = performance.now();
        syncAccessible();
        observer = new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) { if (frame !== null) { cancelAnimationFrame(frame); frame = null; } }
                else if (frame === null) { phaseStart = performance.now(); frame = requestAnimationFrame(tick); }
            }
        }, { threshold: 0.1 });
        observer.observe(element);
    };

    const mediaHandler = () => { if (!destroyed) configure(); };
    media?.addEventListener('change', mediaHandler);
    configure();

    return {
        update(next) { options = next; configure(); },
        setDocumentVisible() {},
        hasActiveAnimationFrame() { return frame !== null; },
        destroy() {
            destroyed = true;
            if (frame !== null) cancelAnimationFrame(frame);
            if (timer !== null) clearTimeout(timer);
            observer?.disconnect();
            media?.removeEventListener('change', mediaHandler);
            textEl?.remove(); caretEl?.remove();
            textEl = null; caretEl = null;
            const list = lines();
            element.textContent = list.length > 0 ? (list[0] ?? '') : '';
            element.removeAttribute('aria-label');
        },
    };
}
```

- [ ] **Step 4: Register the three factories in the `factories` map**

Edit the `factories` object literal (around line 75) and add three entries at the end (before the closing `}`):

```js
'word-rotate': createWordRotate,
'morph-text': createMorphText,
'typewriter': createTypewriter,
```

- [ ] **Step 5: Verify file is syntactically valid (Node parse)**

Run:
```bash
node --check src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js
```

Expected: exits 0 with no syntax errors. (If `node` is not available, skip and rely on the browser test in Task 8 to surface syntax errors.)

- [ ] **Step 6: Commit**

```bash
git add src/SyntaxCircus.FancyBlazor/wwwroot/js/fancy-blazor.js
git commit -m "feat(core): add word-rotate, morph-text, and typewriter JS factories"
```

---

## Task 2: Create `WordRotate.razor` template and code-behind

**Files:**
- Create: `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor`
- Create: `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor.cs`
- Create: `src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor.css`

**Interfaces:**
- Consumes: `IFancyEffectRuntime` (injected), `AttributeComposer`, `JsonSerializer`.
- Produces: a public `WordRotate` partial class in `SyntaxCircus.FancyBlazor`.

- [ ] **Step 1: Write `WordRotate.razor`**

```razor
@namespace SyntaxCircus.FancyBlazor
<span @ref="_element" @attributes="RootAttributes"></span>
```

- [ ] **Step 2: Write `WordRotate.razor.cs`**

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Cycles through a list of headline words with a transition between each word while keeping visual text decorative.</summary>
public partial class WordRotate : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<string> Words { get; set; } = Array.Empty<string>();

    [Parameter] public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(2.5);
    [Parameter] public bool Loop { get; set; } = true;
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public WordRotateTransition Transition { get; set; } = WordRotateTransition.Fade;
    [Parameter] public string? Easing { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-word-rotate", CssClass,
        null, Style, AdditionalAttributes,
        BuildFixedAttributes());

    private Dictionary<string, object> BuildFixedAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "word-rotate",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-word-rotate-transition"] = Transition switch
            {
                WordRotateTransition.Fade => "fade",
                WordRotateTransition.SlideUp => "slide-up",
                WordRotateTransition.SlideDown => "slide-down",
                WordRotateTransition.Blur => "blur",
                _ => "fade",
            },
        };
        if (Disabled)
        {
            attrs["class"] = "syntax-circus-fancy-kinetic-text--static";
        }
        return attrs;
    }

    protected override void OnInitialized()
    {
        if (Words is null || Words.Count < 2)
        {
            throw new InvalidOperationException("WordRotate requires at least two words.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var intervalMs = Math.Clamp(AttributeComposer.NonNegative(Interval).TotalMilliseconds, 250, 30000);
        var options = new
        {
            words = Words,
            interval = intervalMs,
            loop = Loop,
            startIndex = Math.Max(0, StartIndex),
            transition = Transition.ToString().ToLowerInvariant(),
            easing = Easing,
        };
        var signature = JsonSerializer.Serialize(options);

        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "word-rotate", options).ConfigureAwait(false);
            _signature = signature;
        }
        else if (_signature != signature)
        {
            await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false);
            _signature = signature;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DestroyAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DestroyAsync()
    {
        if (_handle is not { } handle) return;
        _handle = null;
        _signature = null;
        await Runtime.DestroyAsync(handle).ConfigureAwait(false);
    }
}
```

- [ ] **Step 3: Write `WordRotate.razor.css`**

```css
.syntax-circus-fancy-word-rotate { display: inline-block; position: relative; color: var(--sc-fancy-kinetic-text-color, currentColor); }
.syntax-circus-fancy-word-rotate__display { display: inline-block; transition: opacity var(--sc-fancy-word-rotate-duration, 320ms) var(--sc-fancy-kinetic-text-easing, ease-out), transform var(--sc-fancy-word-rotate-duration, 320ms) var(--sc-fancy-kinetic-text-easing, ease-out), filter var(--sc-fancy-word-rotate-duration, 320ms) var(--sc-fancy-kinetic-text-easing, ease-out); }
.syntax-circus-fancy-word-rotate__display[data-fancy-state="in"] { opacity: 1; transform: translateY(0); filter: blur(0); }
.syntax-circus-fancy-word-rotate__display[data-fancy-state="out"] { opacity: 0; }
.syntax-circus-fancy-word-rotate[data-fancy-word-rotate-transition="slide-up"] .syntax-circus-fancy-word-rotate__display[data-fancy-state="out"] { transform: translateY(0.5em); }
.syntax-circus-fancy-word-rotate[data-fancy-word-rotate-transition="slide-down"] .syntax-circus-fancy-word-rotate__display[data-fancy-state="out"] { transform: translateY(-0.5em); }
.syntax-circus-fancy-word-rotate[data-fancy-word-rotate-transition="blur"] .syntax-circus-fancy-word-rotate__display[data-fancy-state="out"] { filter: blur(6px); }
.syntax-circus-fancy-word-rotate.syntax-circus-fancy-kinetic-text--static::after { content: ''; }
@media (prefers-reduced-motion: reduce) { .syntax-circus-fancy-word-rotate__display { transition: none; } }
```

- [ ] **Step 4: Build to verify compilation**

Run:
```bash
dotnet build src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --configuration Release
```

Expected: succeeds with no errors. (The `WordRotateTransition` enum will fail because it doesn't exist yet — fix in next step.)

- [ ] **Step 5: Create `WordRotateTransition` enum**

Create file `src/SyntaxCircus.FancyBlazor/WordRotateTransition.cs`:

```csharp
namespace SyntaxCircus.FancyBlazor;

/// <summary>Visual transition applied when a <see cref="WordRotate"/> cycle swaps words.</summary>
public enum WordRotateTransition
{
    Fade,
    SlideUp,
    SlideDown,
    Blur,
}
```

- [ ] **Step 6: Re-build to confirm clean compile**

Run:
```bash
dotnet build src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --configuration Release
```

Expected: succeeds.

- [ ] **Step 7: Commit**

```bash
git add src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor.cs src/SyntaxCircus.FancyBlazor/Components/WordRotate.razor.css src/SyntaxCircus.FancyBlazor/WordRotateTransition.cs
git commit -m "feat(core): add WordRotate component"
```

---

## Task 3: Create `MorphText.razor` template and code-behind

**Files:**
- Create: `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor`
- Create: `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor.cs`
- Create: `src/SyntaxCircus.FancyBlazor/Components/MorphText.razor.css`
- Create: `src/SyntaxCircus.FancyBlazor/MorphMode.cs`

**Interfaces:**
- Consumes: same as Task 2.
- Produces: `MorphText` partial class and `MorphMode` enum.

- [ ] **Step 1: Write `MorphText.razor`**

```razor
@namespace SyntaxCircus.FancyBlazor
<span @ref="_element" @attributes="RootAttributes"></span>
```

- [ ] **Step 2: Write `MorphText.razor.cs`**

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Crossfades or character-splits between a list of strings while holding each for a visible beat.</summary>
public partial class MorphText : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<string> Words { get; set; } = Array.Empty<string>();

    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(600);
    [Parameter] public TimeSpan Hold { get; set; } = TimeSpan.FromSeconds(1.2);
    [Parameter] public bool Loop { get; set; } = true;
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public MorphMode Mode { get; set; } = MorphMode.Crossfade;
    [Parameter] public string? Easing { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-morph-text", CssClass,
        null, Style, AdditionalAttributes,
        BuildFixedAttributes());

    private Dictionary<string, object> BuildFixedAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "morph-text",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-morph-mode"] = Mode == MorphMode.CharSplit ? "char-split" : "crossfade",
        };
        if (Disabled)
        {
            attrs["class"] = "syntax-circus-fancy-kinetic-text--static";
        }
        return attrs;
    }

    protected override void OnInitialized()
    {
        if (Words is null || Words.Count < 2)
        {
            throw new InvalidOperationException("MorphText requires at least two words.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var durationMs = Math.Clamp(AttributeComposer.NonNegative(Duration).TotalMilliseconds, 100, 2000);
        var holdMs = Math.Clamp(AttributeComposer.NonNegative(Hold).TotalMilliseconds, 0, 10000);
        var options = new
        {
            words = Words,
            duration = durationMs,
            hold = holdMs,
            loop = Loop,
            startIndex = Math.Max(0, StartIndex),
            mode = Mode == MorphMode.CharSplit ? "char-split" : "crossfade",
            easing = Easing,
        };
        var signature = JsonSerializer.Serialize(options);

        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "morph-text", options).ConfigureAwait(false);
            _signature = signature;
        }
        else if (_signature != signature)
        {
            await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false);
            _signature = signature;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DestroyAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DestroyAsync()
    {
        if (_handle is not { } handle) return;
        _handle = null;
        _signature = null;
        await Runtime.DestroyAsync(handle).ConfigureAwait(false);
    }
}
```

- [ ] **Step 3: Write `MorphText.razor.css`**

```css
.syntax-circus-fancy-morph-text { display: inline-block; position: relative; color: var(--sc-fancy-kinetic-text-color, currentColor); min-height: 1em; }
.syntax-circus-fancy-morph-text__layer { position: absolute; inset: 0; transition: opacity var(--sc-fancy-morph-text-duration, 320ms) var(--sc-fancy-kinetic-text-easing, cubic-bezier(0.22, 1, 0.36, 1)), transform var(--sc-fancy-morph-text-duration, 320ms) var(--sc-fancy-kinetic-text-easing, cubic-bezier(0.22, 1, 0.36, 1)); white-space: nowrap; }
.syntax-circus-fancy-morph-text__layer[data-fancy-state="in"] { opacity: 1; transform: translateY(0); }
.syntax-circus-fancy-morph-text__layer[data-fancy-state="out"] { opacity: 0; transform: translateY(-0.25em); }
.syntax-circus-fancy-morph-text__layer[data-fancy-state="idle"] { opacity: 0; transform: translateY(0.25em); }
.syntax-circus-fancy-morph-text[data-fancy-morph-mode="char-split"] .syntax-circus-fancy-morph-text__layer[data-fancy-state="in"] { color: var(--sc-fancy-palette-accent, currentColor); }
@media (prefers-reduced-motion: reduce) { .syntax-circus-fancy-morph-text__layer { transition: none; } }
```

- [ ] **Step 4: Create `MorphMode.cs`**

```csharp
namespace SyntaxCircus.FancyBlazor;

/// <summary>Visual mode applied by <see cref="MorphText"/> when transitioning between words.</summary>
public enum MorphMode
{
    Crossfade,
    CharSplit,
}
```

- [ ] **Step 5: Build to verify**

Run:
```bash
dotnet build src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --configuration Release
```

Expected: succeeds.

- [ ] **Step 6: Commit**

```bash
git add src/SyntaxCircus.FancyBlazor/Components/MorphText.razor src/SyntaxCircus.FancyBlazor/Components/MorphText.razor.cs src/SyntaxCircus.FancyBlazor/Components/MorphText.razor.css src/SyntaxCircus.FancyBlazor/MorphMode.cs
git commit -m "feat(core): add MorphText component"
```

---

## Task 4: Create `Typewriter.razor` template and code-behind

**Files:**
- Create: `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor`
- Create: `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor.cs`
- Create: `src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor.css`
- Create: `src/SyntaxCircus.FancyBlazor/KineticTextDirection.cs`

**Interfaces:**
- Consumes: same as Task 2.
- Produces: `Typewriter` partial class and `KineticTextDirection` enum.

- [ ] **Step 1: Write `Typewriter.razor`**

```razor
@namespace SyntaxCircus.FancyBlazor
<span @ref="_element" @attributes="RootAttributes"></span>
```

- [ ] **Step 2: Write `Typewriter.razor.cs`**

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Progressively types a list of lines character by character with an optional blinking caret.</summary>
public partial class Typewriter : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<string> Text { get; set; } = Array.Empty<string>();

    [Parameter] public TimeSpan Speed { get; set; } = TimeSpan.FromMilliseconds(60);
    [Parameter] public TimeSpan HoldAfter { get; set; } = TimeSpan.FromSeconds(1.5);
    [Parameter] public TimeSpan? DeleteSpeed { get; set; }
    [Parameter] public bool Loop { get; set; } = true;
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public bool Caret { get; set; } = true;
    [Parameter] public string CaretCharacter { get; set; } = "|";
    [Parameter] public KineticTextDirection Direction { get; set; } = KineticTextDirection.Auto;

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-typewriter", CssClass,
        null, Style, AdditionalAttributes,
        BuildFixedAttributes());

    private Dictionary<string, object> BuildFixedAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "typewriter",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-typewriter-caret"] = Caret ? "true" : "false",
            ["data-fancy-typewriter-direction"] = Direction.ToString().ToLowerInvariant(),
        };
        if (Disabled)
        {
            attrs["class"] = "syntax-circus-fancy-kinetic-text--static";
        }
        return attrs;
    }

    protected override void OnInitialized()
    {
        if (Text is null || Text.Count == 0)
        {
            throw new InvalidOperationException("Typewriter requires at least one line.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var speedMs = Math.Clamp(AttributeComposer.NonNegative(Speed).TotalMilliseconds, 10, 500);
        var holdAfterMs = Math.Clamp(AttributeComposer.NonNegative(HoldAfter).TotalMilliseconds, 0, 30000);
        var options = new
        {
            text = Text,
            speed = speedMs,
            holdAfter = holdAfterMs,
            deleteSpeed = DeleteSpeed is null ? (double?)null : Math.Clamp(AttributeComposer.NonNegative(DeleteSpeed.Value).TotalMilliseconds, 10, 500),
            loop = Loop,
            startIndex = Math.Max(0, StartIndex),
            caret = Caret,
            caretCharacter = CaretCharacter,
        };
        var signature = JsonSerializer.Serialize(options);

        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "typewriter", options).ConfigureAwait(false);
            _signature = signature;
        }
        else if (_signature != signature)
        {
            await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false);
            _signature = signature;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DestroyAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DestroyAsync()
    {
        if (_handle is not { } handle) return;
        _handle = null;
        _signature = null;
        await Runtime.DestroyAsync(handle).ConfigureAwait(false);
    }
}
```

- [ ] **Step 3: Write `Typewriter.razor.css`**

```css
.syntax-circus-fancy-typewriter { display: inline-block; position: relative; color: var(--sc-fancy-kinetic-text-color, currentColor); }
.syntax-circus-fancy-typewriter__text { white-space: pre; }
.syntax-circus-fancy-typewriter__caret { display: inline-block; width: 0.06em; margin-left: 0.05em; color: var(--sc-fancy-typewriter-caret-color, var(--sc-fancy-palette-accent, currentColor)); animation: syntax-circus-fancy-typewriter-caret-blink var(--sc-fancy-typewriter-caret-blink-duration, 1s) steps(2, end) infinite; vertical-align: baseline; }
.syntax-circus-fancy-typewriter[data-fancy-typewriter-caret="false"] .syntax-circus-fancy-typewriter__caret { display: none; }
@keyframes syntax-circus-fancy-typewriter-caret-blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
@media (prefers-reduced-motion: reduce) { .syntax-circus-fancy-typewriter__caret { animation: none; opacity: 1; } }
```

- [ ] **Step 4: Create `KineticTextDirection.cs`**

```csharp
namespace SyntaxCircus.FancyBlazor;

/// <summary>Reading direction for kinetic text effects.</summary>
public enum KineticTextDirection
{
    Auto,
    Ltr,
    Rtl,
}
```

- [ ] **Step 5: Build to verify**

Run:
```bash
dotnet build src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --configuration Release
```

Expected: succeeds.

- [ ] **Step 6: Commit**

```bash
git add src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor.cs src/SyntaxCircus.FancyBlazor/Components/Typewriter.razor.css src/SyntaxCircus.FancyBlazor/KineticTextDirection.cs
git commit -m "feat(core): add Typewriter component"
```

---

## Task 5: Add bUnit component contract tests

**Files:**
- Modify: `tests/SyntaxCircus.FancyBlazor.Tests/ComponentContractTests.cs`

**Interfaces:**
- Consumes: existing `CreateContext()` helper at the bottom of the file; `using` directives already in the file.
- Produces: four new `[Fact]` methods:
  - `WordRotate_RendersSemanticHostWithAccessibleText`
  - `MorphText_RendersSemanticHostWithAccessibleText`
  - `Typewriter_RendersSemanticHostWithAccessibleText`
  - `KineticTextComponents_Disabled_AddStaticClassAndDoNotInvokeRuntime`

- [ ] **Step 1: Add `WordRotate_RendersSemanticHostWithAccessibleText`**

Insert after the existing `ScrambleText_RendersSemanticElementWithAccessibleText` test. Use:

```csharp
[Fact]
public void WordRotate_RendersSemanticHostWithAccessibleText()
{
    using var context = CreateContext();

    var markup = context.Render<WordRotate>(p => p
        .Add(x => x.Words, new[] { "Designers", "Developers", "Dreamers" })
        .Add(x => x.Interval, TimeSpan.FromMilliseconds(750))
        .Add(x => x.Transition, WordRotateTransition.SlideUp)
        .Add(x => x.CssClass, "hero-rotate")).Markup;

    markup.ShouldContain("syntax-circus-fancy-word-rotate hero-rotate");
    markup.ShouldContain("data-fancy-effect=\"word-rotate\"");
    markup.ShouldContain("data-fancy-word-rotate-transition=\"slide-up\"");
    markup.ShouldContain("Designers");
}

[Fact]
public void WordRotate_RequiresAtLeastTwoWords()
{
    using var context = CreateContext();

    Should.Throw<InvalidOperationException>(() => context.Render<WordRotate>(p => p.Add(x => x.Words, new[] { "only" })));
}
```

- [ ] **Step 2: Add `MorphText_RendersSemanticHostWithAccessibleText`**

```csharp
[Fact]
public void MorphText_RendersSemanticHostWithAccessibleText()
{
    using var context = CreateContext();

    var markup = context.Render<MorphText>(p => p
        .Add(x => x.Words, new[] { "Compose", "Animate", "Ship" })
        .Add(x => x.Duration, TimeSpan.FromMilliseconds(400))
        .Add(x => x.Hold, TimeSpan.FromMilliseconds(900))
        .Add(x => x.Mode, MorphMode.CharSplit)).Markup;

    markup.ShouldContain("syntax-circus-fancy-morph-text");
    markup.ShouldContain("data-fancy-effect=\"morph-text\"");
    markup.ShouldContain("data-fancy-morph-mode=\"char-split\"");
    markup.ShouldContain("Compose");
}

[Fact]
public void MorphText_RequiresAtLeastTwoWords()
{
    using var context = CreateContext();

    Should.Throw<InvalidOperationException>(() => context.Render<MorphText>(p => p.Add(x => x.Words, Array.Empty<string>())));
}
```

- [ ] **Step 3: Add `Typewriter_RendersSemanticHostWithAccessibleText`**

```csharp
[Fact]
public void Typewriter_RendersSemanticHostWithAccessibleText()
{
    using var context = CreateContext();

    var markup = context.Render<Typewriter>(p => p
        .Add(x => x.Text, new[] { "Hello", "World" })
        .Add(x => x.Speed, TimeSpan.FromMilliseconds(30))
        .Add(x => x.CaretCharacter, "_")
        .Add(x => x.Direction, KineticTextDirection.Ltr)).Markup;

    markup.ShouldContain("syntax-circus-fancy-typewriter");
    markup.ShouldContain("data-fancy-effect=\"typewriter\"");
    markup.ShouldContain("data-fancy-typewriter-caret=\"true\"");
    markup.ShouldContain("data-fancy-typewriter-direction=\"ltr\"");
    markup.ShouldContain("Hello");
}

[Fact]
public void Typewriter_RequiresAtLeastOneLine()
{
    using var context = CreateContext();

    Should.Throw<InvalidOperationException>(() => context.Render<Typewriter>(p => p.Add(x => x.Text, Array.Empty<string>())));
}
```

- [ ] **Step 4: Add `KineticTextComponents_Disabled_AddStaticClassAndDoNotInvokeRuntime`**

```csharp
[Fact]
public void KineticTextComponents_Disabled_AddStaticClassAndDoNotInvokeRuntime()
{
    using var context = CreateContext();

    var rotate = context.Render<WordRotate>(p => p.Add(x => x.Words, new[] { "A", "B" }).Add(x => x.Disabled, true)).Markup;
    var morph = context.Render<MorphText>(p => p.Add(x => x.Words, new[] { "A", "B" }).Add(x => x.Disabled, true)).Markup;
    var typewriter = context.Render<Typewriter>(p => p.Add(x => x.Text, new[] { "Only" }).Add(x => x.Disabled, true)).Markup;

    rotate.ShouldContain("syntax-circus-fancy-kinetic-text--static");
    morph.ShouldContain("syntax-circus-fancy-kinetic-text--static");
    typewriter.ShouldContain("syntax-circus-fancy-kinetic-text--static");
    rotate.ShouldContain("data-fancy-disabled=\"true\"");
    morph.ShouldContain("data-fancy-disabled=\"true\"");
    typewriter.ShouldContain("data-fancy-disabled=\"true\"");
}
```

- [ ] **Step 5: Run the new tests**

Run:
```bash
dotnet test tests/SyntaxCircus.FancyBlazor.Tests/SyntaxCircus.FancyBlazor.Tests.csproj --filter "FullyQualifiedName~ComponentContractTests.WordRotate|FullyQualifiedName~ComponentContractTests.MorphText|FullyQualifiedName~ComponentContractTests.Typewriter|FullyQualifiedName~ComponentContractTests.KineticText" --configuration Release
```

Expected: all seven new tests pass.

- [ ] **Step 6: Commit**

```bash
git add tests/SyntaxCircus.FancyBlazor.Tests/ComponentContractTests.cs
git commit -m "test(core): add bUnit contract tests for WordRotate, MorphText, Typewriter"
```

---

## Task 6: Add the demo route

**Files:**
- Create: `samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor`

- [ ] **Step 1: Write the showcase page**

```razor
@page "/kinetic-text"
@using SyntaxCircus.FancyBlazor

<PageTitle>FancyBlazor · Kinetic text</PageTitle>

<Reveal Effect="RevealEffect.FadeUp">
    <header class="page-header">
        <p class="kicker">CORE EFFECTS / KINETIC TEXT</p>
        <h1>WordRotate, MorphText, Typewriter.</h1>
        <p class="lede">Three decorative text effects that share a lifecycle but solve distinct problems. Each keeps the visible motion aria-hidden and exposes a visually-hidden accessible mirror.</p>
    </header>
</Reveal>

<section class="demo-page">
    <h2>WordRotate</h2>
    <p class="catalog-section__lede">Cycles through a list of headline words with a transition between each.</p>
    <Reveal>
        <p class="kinetic-paragraph">
            Built for
            <WordRotate Words="@(new[] { "designers", "developers", "dreamers", "deployers" })" Interval="TimeSpan.FromSeconds(2)" Transition="WordRotateTransition.Fade" />
            who care about the in-between.
        </p>
    </Reveal>
    <Reveal>
        <p class="kinetic-paragraph">
            <WordRotate Words="@(new[] { "Compose", "Animate", "Ship" })" Interval="TimeSpan.FromSeconds(1.5)" Transition="WordRotateTransition.SlideUp" CssClass="kinetic-rotate--uppercase" />
        </p>
    </Reveal>
    <Reveal>
        <p class="kinetic-paragraph">
            <WordRotate Words="@(new[] { "Focus", "Flow", "Finish" })" Interval="TimeSpan.FromSeconds(1.5)" Transition="WordRotateTransition.Blur" />
        </p>
    </Reveal>
</section>

<section class="demo-page">
    <h2>MorphText</h2>
    <p class="catalog-section__lede">Crossfades or character-splits between strings with a visible hold between each.</p>
    <Reveal>
        <p class="kinetic-paragraph">
            <MorphText Words="@(new[] { "Compose", "Animate", "Ship" })" Duration="TimeSpan.FromMilliseconds(450)" Hold="TimeSpan.FromSeconds(1.1)" />
        </p>
    </Reveal>
    <Reveal>
        <p class="kinetic-paragraph">
            <MorphText Words="@(new[] { "Frosted", "Faceted", "Filament" })" Mode="MorphMode.CharSplit" Hold="TimeSpan.FromSeconds(1.4)" />
        </p>
    </Reveal>
</section>

<section class="demo-page">
    <h2>Typewriter</h2>
    <p class="catalog-section__lede">Progressively types a list of lines with an optional caret and optional delete.</p>
    <Reveal>
        <h3 class="kinetic-headline">
            <Typewriter Text="@(new[] { "Hello, world.", "Compose with FancyBlazor.", "Ship something fancy." })" Speed="TimeSpan.FromMilliseconds(55)" HoldAfter="TimeSpan.FromSeconds(1.2)" />
        </h3>
    </Reveal>
    <Reveal>
        <p class="kinetic-paragraph">
            <Typewriter Text="@(new[] { "Type.", "Delete.", "Retype." })" Speed="TimeSpan.FromMilliseconds(60)" DeleteSpeed="TimeSpan.FromMilliseconds(30)" HoldAfter="TimeSpan.FromMilliseconds(900)" CaretCharacter="_" />
        </p>
    </Reveal>
    <Reveal>
        <p class="kinetic-paragraph">
            <Typewriter Text="@(new[] { "One line, no caret." })" Caret="false" />
        </p>
    </Reveal>
</section>

<section class="demo-page">
    <h2>Reduced motion</h2>
    <p class="catalog-section__lede">All three settle to the first word or line and stop animating.</p>
    <Reveal>
        <p class="kinetic-paragraph">
            <WordRotate Words="@(new[] { "Static", "Settled", "Still" })" Interval="TimeSpan.FromSeconds(2)" Disabled="@_disabled" />
            <MorphText Words="@(new[] { "Static", "Settled", "Still" })" Hold="TimeSpan.FromSeconds(1)" Disabled="@_disabled" />
            <Typewriter Text="@(new[] { "Static line." })" Disabled="@_disabled" />
        </p>
    </Reveal>
    <label class="kinetic-toggle">
        <input type="checkbox" @bind="_disabled" /> <span>Disable kinetic motion (simulates prefers-reduced-motion)</span>
    </label>
</section>

<section class="demo-page">
    <h2>Composition</h2>
    <p class="catalog-section__lede">WordRotate inside a Hero and a CallToAction.</p>
    <Hero Alignment="HeroAlignment.Center" CssClass="kinetic-hero">
        <p class="kicker">COMPOSE / ANIMATE / SHIP</p>
        <h2>
            <WordRotate Words="@(new[] { "Compose", "Animate", "Ship" })" Interval="TimeSpan.FromSeconds(1.8)" />
        </h2>
        <p>One of three verbs, cycling on a timer.</p>
    </Hero>
    <CallToAction Layout="CallToActionLayout.Inline">
        <h3>
            <Typewriter Text="@(new[] { "Ready when you are." })" Speed="TimeSpan.FromMilliseconds(45)" />
        </h3>
    </CallToAction>
</section>

<section class="demo-page">
    <h2>Lifecycle</h2>
    <p class="catalog-section__lede">Remove the components to verify observers and timers are released.</p>
    <Reveal>
        <p class="kinetic-paragraph" data-testid="kinetic-lifecycle-host">
            @if (_showLifecycle)
            {
                <WordRotate Words="@(new[] { "Removing", "Soon" })" Interval="TimeSpan.FromMilliseconds(500)" />
                <Typewriter Text="@(new[] { "Type me" })" Speed="TimeSpan.FromMilliseconds(20)" />
            }
        </p>
    </Reveal>
    <button type="button" data-testid="kinetic-lifecycle-toggle" @onclick="() => _showLifecycle = !_showLifecycle">@(_showLifecycle ? "Remove" : "Restore")</button>
    <span data-testid="kinetic-lifecycle-state">@(_showLifecycle ? "mounted" : "removed")</span>
</section>

@code {
    private bool _disabled;
    private bool _showLifecycle = true;
}
```

- [ ] **Step 2: Build the demo project to verify**

Run:
```bash
dotnet build samples/FancyBlazor.Demo.Client/FancyBlazor.Demo.Client.csproj --configuration Release
```

Expected: succeeds.

- [ ] **Step 3: Commit**

```bash
git add samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor
git commit -m "feat(demo): add KineticTextShowcase route"
```

---

## Task 7: Link the route from primary nav, footer nav, home effect-grid, and home catalog directory

**Files:**
- Modify: `samples/FancyBlazor.Demo.Client/Layout/MainLayout.razor`
- Modify: `samples/FancyBlazor.Demo.Client/Pages/Home.razor`
- Modify: `tests/SyntaxCircus.FancyBlazor.Tests/DemoCatalogTests.cs`

- [ ] **Step 1: Add the link to the header nav in `MainLayout.razor`**

In the "More to explore" group (around line 30), add a new `<NavLink>` after `<NavLink href="/core-kinetic-catalog" ...>Core kinetic catalog</NavLink>`:

```razor
<NavLink href="/kinetic-text" @onclick="CloseMenu">Kinetic text</NavLink>
```

- [ ] **Step 2: Add the link to the footer nav in `MainLayout.razor`**

In the matching footer "More to explore" group (around line 71), add a new `<NavLink>` after the same line:

```razor
<NavLink href="/kinetic-text">Kinetic text</NavLink>
```

- [ ] **Step 3: Add a card to the home effect-grid**

In `Home.razor`, add a new card to `.effect-grid` (the 7-card grid). Insert it after the "TYPOGRAPHY / Voice" card. The new card is for kinetic text:

```razor
<a href="/kinetic-text"><span>KINETIC TEXT</span><strong>Motion</strong><small>Word cycling, morphing, and typing for headlines</small></a>
```

- [ ] **Step 4: Add a row to the home catalog directory under "Voice"**

In `Home.razor`, inside the `<section>` with `<h3>Voice</h3>` (around line 74-78), add a new anchor after the existing two:

```razor
<a href="/kinetic-text"><span class="catalog-directory__groups-title">Kinetic text</span><span class="catalog-directory__groups-meta">Word rotate, morph, and typewriter</span></a>
```

- [ ] **Step 5: Update `DemoDestinations` array in `DemoCatalogTests.cs`**

Edit the array to insert `"/kinetic-text"` at the correct position. The `Home_CatalogDirectory_LinksToEveryDemoDestination` test does strict array equality, so the order must match. The catalog directory order under "Voice" will be: expressive-effects, css-first-catalog, **kinetic-text**. So the new array is:

```csharp
private static readonly string[] DemoDestinations =
[
    "/background", "/expanded-effects", "/threeui-inspiration", "/core-kinetic-catalog",
    "/border", "/spatial-surfaces", "/webgl",
    "/reveal", "/tilt", "/narrative-motion",
    "/expressive-effects", "/css-first-catalog", "/kinetic-text",
    "/interaction-feedback",
    "/composition-authoring",
    "/ui-companion", "/marketing",
];
```

- [ ] **Step 6: Run the catalog test**

Run:
```bash
dotnet test tests/SyntaxCircus.FancyBlazor.Tests/SyntaxCircus.FancyBlazor.Tests.csproj --filter "FullyQualifiedName~DemoCatalogTests" --configuration Release
```

Expected: all `DemoCatalogTests` pass, including the strict-array test.

- [ ] **Step 7: Build the demo project**

Run:
```bash
dotnet build samples/FancyBlazor.Demo.Client/FancyBlazor.Demo.Client.csproj --configuration Release
```

Expected: succeeds.

- [ ] **Step 8: Commit**

```bash
git add samples/FancyBlazor.Demo.Client/Layout/MainLayout.razor samples/FancyBlazor.Demo.Client/Pages/Home.razor tests/SyntaxCircus.FancyBlazor.Tests/DemoCatalogTests.cs
git commit -m "feat(demo): link KineticTextShowcase from nav, footer, and home"
```

---

## Task 8: Add a Playwright browser test

**Files:**
- Modify: `tests/SyntaxCircus.FancyBlazor.BrowserTests/FancyBlazorBrowserTests.cs`

**Interfaces:**
- Consumes: existing `fixture.TestHostUrl`; existing Playwright patterns in the file.
- Produces: one new `[Fact]` method `KineticTextShowcase_RendersCyclesAndIsLinkedFromFourPlaces`.

- [ ] **Step 1: Add the test**

Append the new test at the end of the `FancyBlazorBrowserTests` class:

```csharp
[Fact]
public async Task KineticTextShowcase_RendersCyclesAndIsLinkedFromFourPlaces()
{
    using var client = new HttpClient();
    var html = await client.GetStringAsync($"{fixture.TestHostUrl}/kinetic-text", TestContext.Current.CancellationToken);
    html.ShouldContain("WordRotate");
    html.ShouldContain("MorphText");
    html.ShouldContain("Typewriter");
    html.ShouldContain("syntax-circus-fancy-word-rotate");
    html.ShouldContain("syntax-circus-fancy-morph-text");
    html.ShouldContain("syntax-circus-fancy-typewriter");
    html.ShouldNotContain("data-fancy-state=\"out\"");

    var home = await client.GetStringAsync($"{fixture.TestHostUrl}/", TestContext.Current.CancellationToken);
    var occurrences = System.Text.RegularExpressions.Regex.Matches(home, "href=\"/kinetic-text\"").Count;
    occurrences.ShouldBeGreaterThanOrEqualTo(3);

    await using var browser = await NewWebGlBrowserAsync();
    await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { ReducedMotion = ReducedMotion.Reduce });
    var page = await context.NewPageAsync();
    await page.GotoAsync($"{fixture.TestHostUrl}/kinetic-text");
    await page.WaitForTimeoutAsync(200);
    var reduceWordRotate = await page.Locator(".syntax-circus-fancy-word-rotate").First().GetAttributeAsync("data-fancy-disabled");
    reduceWordRotate.ShouldBe("false");
    (await page.Locator(".syntax-circus-fancy-typewriter").First().GetAttributeAsync("data-fancy-disabled")).ShouldBe("false");

    await page.Locator("[data-testid='kinetic-lifecycle-toggle']").ClickAsync();
    await page.WaitForTimeoutAsync(150);
    var hostHtml = await page.Locator("[data-testid='kinetic-lifecycle-host']").InnerHTMLAsync();
    hostHtml.ShouldNotContain("syntax-circus-fancy-word-rotate");
    hostHtml.ShouldNotContain("syntax-circus-fancy-typewriter");

    var diagnostics = await page.EvaluateAsync<object>("async () => globalThis.__syntaxCircusFancyBlazor.getDiagnostics()");
    diagnostics.ShouldNotBeNull();
}
```

- [ ] **Step 2: Run the new browser test**

Run:
```bash
dotnet test tests/SyntaxCircus.FancyBlazor.BrowserTests/SyntaxCircus.FancyBlazor.BrowserTests.csproj --filter "FullyQualifiedName~KineticTextShowcase" --configuration Release
```

Expected: passes. (If Playwright browsers are not installed, first run `pwsh tests/SyntaxCircus.FancyBlazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium`.)

- [ ] **Step 3: Commit**

```bash
git add tests/SyntaxCircus.FancyBlazor.BrowserTests/FancyBlazorBrowserTests.cs
git commit -m "test(core): add browser test for KineticTextShowcase"
```

---

## Task 9: Documentation updates

**Files:**
- Modify: `README.md`
- Create: `docs/components/word-rotate.md`
- Create: `docs/components/morph-text.md`
- Create: `docs/components/typewriter.md`
- Create: `docs/components/kinetic-text-overview.md`
- Modify: `docs/guides/performance.md`
- Modify: `docs/guides/accessibility.md`
- Modify: `docs/architecture/99-IMPLEMENTATION-ROADMAP.md`
- Modify: `docs/architecture/00-DISCOVERY-INDEX.md`
- Modify: `docs/architecture/01-REQUIREMENTS.md`
- Modify: `CHANGELOG.md`
- Modify: `AGENTS.md`

- [ ] **Step 1: Update `README.md`**

In the "Preview components" table, add three new rows. Use the existing row format (e.g. `ScrambleText`, `NumberTicker`):

| Component | Category | One-line description |
| --- | --- | --- |
| `WordRotate` | Text | Cycles a list of headline words with a fade/slide/blur transition while keeping the visible motion decorative. |
| `MorphText` | Text | Crossfades or character-splits between two or more strings with a visible hold between each. |
| `Typewriter` | Text | Progressively types a list of lines character by character with an optional blinking caret. |

Add a code snippet to the "First effect" section (or just below the table):

```razor
<WordRotate Words="@(new[] { "Compose", "Animate", "Ship" })" Interval="TimeSpan.FromSeconds(1.5)" />
```

- [ ] **Step 2: Create `docs/components/word-rotate.md`**

Model the file on the existing `docs/components/scramble-text.md` (read it first). Cover:
- Purpose and one-line description.
- Parameters table.
- Accessibility (aria-hidden visible, visually-hidden mirror, `aria-live="polite"`).
- Reduced motion behavior.
- Code example.
- Composed example inside `Hero` and `CallToAction`.

- [ ] **Step 3: Create `docs/components/morph-text.md`**

Same shape as Step 2, with the morph-specific parameter set.

- [ ] **Step 4: Create `docs/components/typewriter.md`**

Same shape as Step 2, with the typewriter-specific parameter set (including `Caret`, `CaretCharacter`, `DeleteSpeed`).

- [ ] **Step 5: Create `docs/components/kinetic-text-overview.md`**

A short cross-component guide that explains when to pick each: WordRotate for a single "current" word, MorphText for content that changes meaning across variants, Typewriter for sequential line-by-line reveal.

- [ ] **Step 6: Append a section to `docs/guides/performance.md`**

Add a new subsection titled "Kinetic text" that documents:
- One RAF per visible instance.
- Pauses on `prefers-reduced-motion` and on offscreen via `IntersectionObserver`.
- Soft cap of ~12 simultaneous kinetic text instances per page.

- [ ] **Step 7: Append a section to `docs/guides/accessibility.md`**

Add a new subsection titled "Decorative kinetic text" that documents the `aria-hidden` visible-layer + visually-hidden accessible-mirror pattern, with `aria-live` policy per component.

- [ ] **Step 8: Update `docs/architecture/99-IMPLEMENTATION-ROADMAP.md`**

In the core evaluation bank section, remove the `WordRotate` and `MorphText` lines. (If `Typewriter` was not on the bank, leave it alone — it was added as an open-research addition.)

- [ ] **Step 9: Update `docs/architecture/00-DISCOVERY-INDEX.md`**

Add a short discovery note for the new phase. Title it "Phase 19 — Core kinetic text batch" if numbering continues, or add a "Core kinetic text" entry under the most-recently-completed phase.

- [ ] **Step 10: Update `docs/architecture/01-REQUIREMENTS.md`**

In the approved-pre-1.0-roadmap section, add the three new components to the catalog list.

- [ ] **Step 11: Update `CHANGELOG.md`**

In the `Unreleased` section, under `### Added`, add:

```markdown
- `WordRotate` — cycles a list of headline words with a transition between each.
- `MorphText` — crossfades or character-splits between strings with a visible hold.
- `Typewriter` — progressively types a list of lines with an optional caret.
```

- [ ] **Step 12: Update `AGENTS.md`**

In the "Preview components" enumeration (the bullet list near the top), add three bullets:

```markdown
- `WordRotate`, `MorphText`, and `Typewriter` cycle, crossfade, or progressively type decorative text.
```

In the "Purpose and boundary" section's catalog list, add the three components.

In the "Public API rules" section, mention the `--sc-fancy-kinetic-text-*` custom property prefix family.

- [ ] **Step 13: Run the docs verifier**

Run:
```bash
pwsh eng/verify-docs.ps1
```

Expected: exits 0.

- [ ] **Step 14: Commit**

```bash
git add README.md docs/components/word-rotate.md docs/components/morph-text.md docs/components/typewriter.md docs/components/kinetic-text-overview.md docs/guides/performance.md docs/guides/accessibility.md docs/architecture/99-IMPLEMENTATION-ROADMAP.md docs/architecture/00-DISCOVERY-INDEX.md docs/architecture/01-REQUIREMENTS.md CHANGELOG.md AGENTS.md
git commit -m "docs(core): add guides, README, and changelog entries for kinetic text batch"
```

---

## Task 10: Final verification gate

**Files:** none modified; this task is verification only.

- [ ] **Step 1: Restore**

Run:
```bash
dotnet restore SyntaxCircus.FancyBlazor.slnx
```

Expected: succeeds with no errors.

- [ ] **Step 2: Release build**

Run:
```bash
dotnet build SyntaxCircus.FancyBlazor.slnx --no-restore --configuration Release
```

Expected: succeeds with no warnings (or only pre-existing warnings).

- [ ] **Step 3: All tests**

Run:
```bash
dotnet test SyntaxCircus.FancyBlazor.slnx --no-build --configuration Release
```

Expected: all tests pass (bUnit + xUnit).

- [ ] **Step 4: Browser tests**

If not already done: `pwsh tests/SyntaxCircus.FancyBlazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium`

Then:
```bash
dotnet test tests/SyntaxCircus.FancyBlazor.BrowserTests --configuration Release
```

Expected: all Playwright tests pass.

- [ ] **Step 5: Pack**

Run:
```bash
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts/release-preview -p:DisableGitVersionTask=true -p:PackageVersion=0.4.0-preview.1
```

Expected: produces `SyntaxCircus.FancyBlazor.0.4.0-preview.1.nupkg` in `artifacts/release-preview/`.

- [ ] **Step 6: Verify package contents**

Run:
```bash
pwsh eng/verify-package.ps1 -PackageDirectory artifacts/release-preview
```

Expected: exits 0; the three new components are present in the package's `_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js` and the package README has the new rows.

- [ ] **Step 7: Verify docs**

Run:
```bash
pwsh eng/verify-docs.ps1
```

Expected: exits 0.

- [ ] **Step 8: Final report**

After all checks pass, report the commit count, the test count delta, and the new files added. **Do not** open a PR, push, or tag — those require explicit user instruction per AGENTS.md.

---

## Self-Review

**1. Spec coverage** — every section of the spec maps to one or more tasks:
- §1 Purpose: Tasks 1-4 build the three components.
- §3 Architecture: Task 1 extends the existing dispatcher; no new runtime.
- §4 Public API: Tasks 2, 3, 4 each implement one component's full API.
- §6 JavaScript dispatcher: Task 1 implements all three factories.
- §7 Lifecycle: each component file has `OnAfterRenderAsync` / `DisposeAsync` per the existing pattern.
- §8 SSR / non-interactive: factories' `motionReduced` path covers reduced motion; the components render the first item as plain text initially.
- §9 Accessibility: aria-hidden visual layer + accessible mirror pattern in each component.
- §10 Reduced motion: factories honor `prefers-reduced-motion`; components' `Disabled` short-circuits to static.
- §11 Palette integration: handled via CSS custom properties (`var(--sc-fancy-palette-accent, currentColor)`).
- §12 CSS hooks: each component sets the stable hook and the right `data-fancy-*` attributes.
- §16 Resource guarantees: existing dispatcher handles top-level `disposeRuntime`; new factories release all their RAF/timers/observers on `destroy`.
- §19 Testing: Task 5 (bUnit) and Task 8 (browser).
- §20 Demo: Task 6 builds the page; Task 7 wires the four link targets.
- §21 Documentation: Task 9 covers every doc file.
- §22 Test commands: Task 10 runs them in order.

**2. Placeholder scan** — no TBD, no "implement later", no "appropriate error handling". Every step has actual code or actual command.

**3. Type consistency** —
- `IFancyEffectRuntime.CreateAsync(ElementReference, string, object)` is used identically in all three components.
- The factory names `"word-rotate"`, `"morph-text"`, `"typewriter"` appear consistently in the C# code-behind and the JS dispatcher.
- The `KineticTextOptions` shape implied by the C# anonymous objects matches what the JS factories read.
- The DOM class names in the JS factories (`syntax-circus-fancy-word-rotate__display`, `syntax-circus-fancy-morph-text__layer`, `syntax-circus-fancy-typewriter__text`, `syntax-circus-fancy-typewriter__caret`) match the scoped CSS class names in the `.razor.css` files.
- The `data-fancy-*` attribute names are consistent across C# and CSS and JS (none read from JS, but the CSS targets them).
- The CSS custom property names (`--sc-fancy-kinetic-text-*`, `--sc-fancy-word-rotate-duration`, `--sc-fancy-morph-text-duration`, `--sc-fancy-morph-text-hold`, `--sc-fancy-typewriter-caret-color`, `--sc-fancy-typewriter-caret-blink-duration`) appear consistently.

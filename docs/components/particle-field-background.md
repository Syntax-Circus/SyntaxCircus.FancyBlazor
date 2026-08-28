# ParticleFieldBackground (WebGL preview)

`ParticleFieldBackground` renders a decorative, Three.js-backed bounded
particle field behind semantic child content. It ships in the optional
`SyntaxCircus.FancyBlazor.WebGL` preview package, not the core package.

> **Preview API.** The component, parameters, defaults, stable hooks, and visual
> output may change before 1.0.

```bash
dotnet add package SyntaxCircus.FancyBlazor.WebGL
```

```csharp
builder.Services.AddFancyBlazorWebGl();
```

```razor
<ParticleFieldBackground Palette="FancyPalettes.Witchlight"
                          Intensity="0.6"
                          Density="0.5"
                          Size="0.5"
                          Drift="0.5"
                          Speed="0.9"
                          Interactive>
    <article><h1>Semantic content</h1></article>
</ParticleFieldBackground>
```

[Compiling showcase](../../samples/FancyBlazor.Demo.Client/Pages/WebGlShowcase.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `Palette` | `FancyPalette` | `Witchlight` | Supplies the four-color material and CSS fallback. |
| `Intensity` | `double` | `0.5` | Visual strength, clamped to `0..1`. |
| `Density` | `double` | `0.5` | Particle count as a fraction of the quality-tiered cap, clamped to `0..1`. |
| `Size` | `double` | `0.5` | Point-sprite scale, clamped to `0..1`. |
| `Drift` | `double` | `0.5` | Per-particle wander amplitude, clamped to `0..1`. |
| `Speed` | `double` | `1` | Animation multiplier, clamped to `0..3`; zero is static. |
| `Interactive` | `bool` | `false` | Nudges nearby particles toward a fine pointer without capturing events. |
| `Quality` | `FancyQuality?` | `null` | Overrides the shared pixel-density quality and particle-count cap. |
| `Disabled` | `bool` | `false` | Tears down WebGL and displays the CSS fallback. |
| `CssClass` | `string?` | `null` | Adds a class to the outer wrapper. |
| `Style` | `string?` | `null` | Adds inline styles to the outer wrapper. |
| `ChildContent` | `RenderFragment?` | `null` | Semantic content above the canvas. |
| unmatched attributes | — | — | Applied to the outer wrapper. |

`AddFancyBlazorWebGl(options => options.MaxActiveContexts = 2)` sets the
process-wide context ceiling; the default is `4`, and values are clamped to
`1..8`.

| CSS hook or custom property | Purpose |
| --- | --- |
| `.syntax-circus-fancy-particle-field-background` | Stable outer wrapper hook. |
| `.syntax-circus-fancy-particle-field-background__canvas` | Decorative canvas hook. |
| `.syntax-circus-fancy-particle-field-background__content` | Semantic child-content plane. |
| `--sc-fancy-primary`, `--sc-fancy-secondary`, `--sc-fancy-accent`, `--sc-fancy-background` | Palette and fallback colors. |
| `--sc-fancy-particle-field-intensity`, `--sc-fancy-particle-field-density`, `--sc-fancy-particle-field-size`, `--sc-fancy-particle-field-drift`, `--sc-fancy-particle-field-speed` | Current typed material values exposed for styling and inspection. |

The canvas is `aria-hidden`, `tabindex="-1"`, and pointer-transparent. Static
SSR, reduced motion, disabled WebGL, context limits, and renderer failures keep
the child content and a palette-derived CSS treatment. The runtime caps active
contexts, quality-caps DPR, pauses while hidden or offscreen, and releases its
renderer, frame, particle buffers, observers, listeners, and GPU context on
teardown. The particle count is quality-tiered (`Low` 80, `Medium` 160,
`High`/`Auto` 320/160) times `Density`; changing the resolved count rebuilds
the point buffer in place rather than mutating a fixed-size one.

The package vendors unmodified official Three.js r184 ESM files with their MIT
license and SHA-256 provenance. The public component and its visual output
remain preview API until 1.0.

# RefractiveOrbBackground (WebGL preview)

`RefractiveOrbBackground` renders a decorative, Three.js-backed lensing glass
orb behind semantic child content. It ships in the optional
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
<RefractiveOrbBackground Palette="FancyPalettes.Witchlight"
                          Intensity="0.6"
                          Radius="0.55"
                          Distortion="0.5"
                          Sheen="0.65"
                          Speed="0.8"
                          Interactive>
    <article><h1>Semantic content</h1></article>
</RefractiveOrbBackground>
```

[Compiling showcase](../../samples/FancyBlazor.Demo.Client/Pages/WebGlShowcase.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `Palette` | `FancyPalette` | `Witchlight` | Supplies the four-color material and CSS fallback. |
| `Intensity` | `double` | `0.5` | Visual strength, clamped to `0..1`. |
| `Radius` | `double` | `0.5` | Orb size as a fraction of the smaller viewport dimension, clamped to `0..1`. |
| `Distortion` | `double` | `0.5` | Fake-refraction lensing strength, clamped to `0..1`. |
| `Sheen` | `double` | `0.5` | Fresnel rim-highlight strength, clamped to `0..1`. |
| `Speed` | `double` | `1` | Animation multiplier, clamped to `0..3`; zero is static. |
| `Interactive` | `bool` | `false` | Sweeps the rim highlight toward a fine pointer without capturing events. |
| `Quality` | `FancyQuality?` | `null` | Overrides the shared pixel-density quality. |
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
| `.syntax-circus-fancy-refractive-orb-background` | Stable outer wrapper hook. |
| `.syntax-circus-fancy-refractive-orb-background__canvas` | Decorative canvas hook. |
| `.syntax-circus-fancy-refractive-orb-background__content` | Semantic child-content plane. |
| `--sc-fancy-primary`, `--sc-fancy-secondary`, `--sc-fancy-accent`, `--sc-fancy-background` | Palette and fallback colors. |
| `--sc-fancy-refractive-orb-intensity`, `--sc-fancy-refractive-orb-radius`, `--sc-fancy-refractive-orb-distortion`, `--sc-fancy-refractive-orb-sheen`, `--sc-fancy-refractive-orb-speed` | Current typed material values exposed for styling and inspection. |

The canvas is `aria-hidden`, `tabindex="-1"`, and pointer-transparent. Static
SSR, reduced motion, disabled WebGL, context limits, and renderer failures keep
the child content and a palette-derived CSS treatment. The runtime caps active
contexts, quality-caps DPR, pauses while hidden or offscreen, and releases its
renderer, frame, observers, listeners, and GPU context on teardown. The orb is
drawn analytically in a single shader pass — no textures or render targets are
allocated.

The package vendors unmodified official Three.js r184 ESM files with their MIT
license and SHA-256 provenance. The public component and its visual output
remain preview API until 1.0.

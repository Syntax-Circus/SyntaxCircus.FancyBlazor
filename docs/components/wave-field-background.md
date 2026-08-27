# WaveFieldBackground (WebGL preview)

`WaveFieldBackground` renders a decorative, Three.js-backed interference
wave field behind semantic child content. It ships in the optional
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
<WaveFieldBackground Palette="FancyPalettes.Witchlight"
                      Intensity="0.6"
                      Amplitude="0.55"
                      Frequency="0.45"
                      Foam="0.6"
                      Speed="0.9"
                      Interactive>
    <article><h1>Semantic content</h1></article>
</WaveFieldBackground>
```

[Compiling showcase](../../samples/FancyBlazor.Demo.Client/Pages/WebGlShowcase.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `Palette` | `FancyPalette` | `Witchlight` | Supplies the four-color material and CSS fallback. |
| `Intensity` | `double` | `0.5` | Visual strength, clamped to `0..1`. |
| `Amplitude` | `double` | `0.5` | Wave displacement height, clamped to `0..1`. |
| `Frequency` | `double` | `0.5` | Wave tightness/count, clamped to `0..1`. |
| `Foam` | `double` | `0.5` | Bright crest highlight strength, clamped to `0..1`. |
| `Speed` | `double` | `1` | Animation multiplier, clamped to `0..3`; zero is static. |
| `Interactive` | `bool` | `false` | Enables a subtle fine-pointer ripple without capturing events. |
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
| `.syntax-circus-fancy-wave-field-background` | Stable outer wrapper hook. |
| `.syntax-circus-fancy-wave-field-background__canvas` | Decorative canvas hook. |
| `.syntax-circus-fancy-wave-field-background__content` | Semantic child-content plane. |
| `--sc-fancy-primary`, `--sc-fancy-secondary`, `--sc-fancy-accent`, `--sc-fancy-background` | Palette and fallback colors. |
| `--sc-fancy-wave-field-intensity`, `--sc-fancy-wave-field-amplitude`, `--sc-fancy-wave-field-frequency`, `--sc-fancy-wave-field-foam`, `--sc-fancy-wave-field-speed` | Current typed material values exposed for styling and inspection. |

The canvas is `aria-hidden`, `tabindex="-1"`, and pointer-transparent. Static
SSR, reduced motion, disabled WebGL, context limits, and renderer failures keep
the child content and a palette-derived CSS treatment. The runtime caps active
contexts, quality-caps DPR, pauses while hidden or offscreen, and releases its
renderer, frame, observers, listeners, and GPU context on teardown.

The package vendors unmodified official Three.js r184 ESM files with their MIT
license and SHA-256 provenance. The public component and its visual output
remain preview API until 1.0.

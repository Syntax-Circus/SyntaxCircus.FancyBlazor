# ShaderBackground

`ShaderBackground` renders a decorative Nacre shader behind semantic child
content. Static SSR and failure states use a palette-derived CSS background.

```razor
<ShaderBackground Palette="FancyPalettes.Glacier"
                  Intensity="0.65"
                  Interactive="true">
    <section><h1>Real HTML</h1></section>
</ShaderBackground>
```

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Background.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `Effect` | `ShaderEffect` | `Nacre` | Selects the built-in shader. |
| `Palette` | `FancyPalette` | `Witchlight` | Supplies four CSS colors. |
| `Speed` | `double` | `1` | Clamped to `0..3`. |
| `Intensity` | `double` | `0.5` | Clamped to `0..1`. |
| `Interactive` | `bool` | `false` | Enables subtle pointer parallax without capturing events. |
| `Quality` | `FancyQuality?` | `null` | Overrides the global pixel-density ceiling. |
| `Disabled` | `bool` | `false` | Keeps the static fallback and skips WebGL. |
| `CssClass` | `string?` | `null` | Adds a class to the outer wrapper. |
| `Style` | `string?` | `null` | Adds inline styles to the outer wrapper. |
| `ChildContent` | `RenderFragment?` | `null` | Semantic content above the canvas. |
| unmatched attributes | — | — | Applied to the outer wrapper. |

The canvas is `aria-hidden`, `tabindex="-1"`, and pointer-transparent. Reduced
motion does not start WebGL; WebGL errors retain fallback and child content.
Quality caps DPR at 1 (`Low`), 1.5 (`Auto`/`Medium`), or 2 (`High`).

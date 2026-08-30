# TopographicBackground

`TopographicBackground` draws a bounded Canvas 2D field of decorative, slowly drifting topographic contour lines behind ordinary semantic content.

```razor
<TopographicBackground Palette="FancyPalettes.Viridian" Density="5">
    <article>Semantic content</article>
</TopographicBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Viridian` | Four-color field palette. |
| `Density` | `5` | Contour peak cap, clamped to `2..12`. |
| `Speed` | `.12` | Drift speed, clamped to `0..3` (defaults low — the contours are meant to read as static or barely moving). |
| `Intensity` | `.5` | Line brightness, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS radial-gradient background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/AtmosphericBackgrounds.razor)

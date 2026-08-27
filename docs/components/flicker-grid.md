# FlickerGrid

`FlickerGrid` draws a bounded Canvas 2D grid of decorative flickering cells behind ordinary semantic content.

```razor
<FlickerGrid Palette="FancyPalettes.Witchlight" Density="24">
    <article>Semantic content</article>
</FlickerGrid>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Witchlight` | Four-color field palette. |
| `Density` | `28` | Cell cap, clamped to `1..96`. |
| `Speed` | `.35` | Flicker speed, clamped to `0..3`. |
| `Intensity` | `.5` | Flicker brightness, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS grid background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CoreKineticCatalog.razor)

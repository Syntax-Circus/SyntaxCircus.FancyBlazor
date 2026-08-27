# LightRaysBackground

`LightRaysBackground` draws a bounded Canvas 2D sweep of decorative light rays behind ordinary semantic content.

```razor
<LightRaysBackground Palette="FancyPalettes.Glacier" Density="10">
    <article>Semantic content</article>
</LightRaysBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Glacier` | Four-color field palette. |
| `Density` | `10` | Ray cap, clamped to `3..24`. |
| `Speed` | `.35` | Sweep speed, clamped to `0..3`. |
| `Intensity` | `.5` | Ray brightness, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS conic-gradient background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CoreKineticCatalog.razor)

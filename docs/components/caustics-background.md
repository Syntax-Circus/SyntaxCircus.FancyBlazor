# CausticsBackground

`CausticsBackground` draws a bounded Canvas 2D field of decorative drifting caustic light pools behind ordinary semantic content.

```razor
<CausticsBackground Palette="FancyPalettes.Glacier" Density="14">
    <article>Semantic content</article>
</CausticsBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Glacier` | Four-color field palette. |
| `Density` | `14` | Light pool cap, clamped to `1..48`. |
| `Speed` | `.3` | Drift speed, clamped to `0..3`. |
| `Intensity` | `.5` | Pool brightness, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS radial-gradient background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/AtmosphericBackgrounds.razor)

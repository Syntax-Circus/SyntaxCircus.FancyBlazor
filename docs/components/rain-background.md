# RainBackground

`RainBackground` draws a bounded Canvas 2D field of decorative streaking rain behind ordinary semantic content.

```razor
<RainBackground Palette="FancyPalettes.Midnight" Density="60">
    <article>Semantic content</article>
</RainBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Midnight` | Four-color field palette. |
| `Density` | `60` | Droplet cap, clamped to `1..120`. |
| `Speed` | `.6` | Fall speed, clamped to `0..3`. |
| `Intensity` | `.5` | Streak brightness, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS radial-gradient background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/AtmosphericBackgrounds.razor)

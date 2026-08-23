# ConstellationBackground

`ConstellationBackground` draws a bounded Canvas 2D field of decorative points and proximity lines behind ordinary semantic content.

```razor
<ConstellationBackground Palette="FancyPalettes.Witchlight" Density="30">
    <article>Semantic content</article>
</ConstellationBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Witchlight` | Four-color field palette. |
| `Density` | `28` | Point cap, clamped to `1..96`. |
| `Speed` | `.35` | Drift speed, clamped to `0..3`. |
| `LineOpacity` | `.35` | Connection opacity, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)

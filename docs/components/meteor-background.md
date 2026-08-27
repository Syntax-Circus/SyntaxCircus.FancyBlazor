# MeteorBackground

`MeteorBackground` draws a bounded Canvas 2D field of decorative streaking meteors behind ordinary semantic content.

```razor
<MeteorBackground Palette="FancyPalettes.Midnight" Density="16">
    <article>Semantic content</article>
</MeteorBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Midnight` | Four-color field palette. |
| `Density` | `16` | Meteor cap, clamped to `1..48`. |
| `Speed` | `.35` | Streak speed, clamped to `0..3`. |
| `Intensity` | `.5` | Trail brightness, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The canvas is decorative, pointer-transparent, and hidden for reduced motion. A palette-derived CSS diagonal-gradient background remains when JavaScript, Canvas 2D, or motion is unavailable. It pauses while offscreen or hidden and releases observers, frames, and canvas contents on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CoreKineticCatalog.razor)

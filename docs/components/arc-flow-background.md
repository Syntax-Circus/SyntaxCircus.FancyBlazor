# ArcFlowBackground

`ArcFlowBackground` adds a bounded Canvas 2D field of slow decorative arcs behind semantic content.

```razor
<ArcFlowBackground Palette="FancyPalettes.Viridian" Density="16" Intensity=".6">
    <article>Semantic content</article>
</ArcFlowBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Viridian` | Four-color field palette. |
| `Density` | `14` | Arc cap, clamped to `1..64`. |
| `Speed` | `.35` | Drift speed, clamped to `0..3`. |
| `Intensity` | `.5` | Arc visibility, clamped to `0..1`. |
| `Disabled` | `false` | Suppresses Canvas enhancement. |

The Canvas layer is decorative and never captures input. It falls back to a static palette background, honors reduced motion, pauses while hidden or offscreen, and is disposed with its wrapper.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)

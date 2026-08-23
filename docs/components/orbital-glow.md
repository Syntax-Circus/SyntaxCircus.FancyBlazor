# OrbitalGlow

`OrbitalGlow` places a palette-derived light field behind semantic content. It provides one slow, focused movement path; use `AuroraBackground` for a broader multi-light ambient field.

```razor
<OrbitalGlow Palette="FancyPalettes.Viridian" Intensity=".72">
    <section>Semantic content</section>
</OrbitalGlow>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `FancyPalettes.Witchlight` | Four-color background palette. |
| `Intensity` | `.5` | Light strength, clamped to `0..1`. |
| `Animated`, `Duration` | `true`, `20 seconds` | Optional CSS orbit; reduced motion remains static. |
| `Disabled` | `false` | Removes the decorative light layer. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)

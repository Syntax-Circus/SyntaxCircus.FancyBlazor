# AuroraBackground

`AuroraBackground` places palette-derived, blurred CSS light behind semantic content. It drifts continuously; it does not react to pointer movement or scroll.

```razor
<AuroraBackground Palette="FancyPalettes.Viridian"><section>Semantic content</section></AuroraBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `FancyPalettes.Witchlight` | Four-color aurora palette. |
| `Intensity` | `.55` | Light strength, clamped to `0..1`. |
| `Animated`, `Duration` | `true`, `18 seconds` | Optional ambient motion; reduced motion is static. |
| `Disabled` | `false` | Removes the decorative light layer. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)

The aurora is a continuous CSS drift; `NoiseOverlay` is a separate, static texture effect that can be layered over it. The default 18-second motion is intentionally restrained. For a focal surface where movement must be obvious, choose a contrasting palette, increase `Intensity`, or use a shorter `Duration`.

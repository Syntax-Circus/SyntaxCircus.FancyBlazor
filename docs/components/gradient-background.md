# GradientBackground

`GradientBackground` renders a palette-derived CSS background behind semantic content.

```razor
<GradientBackground Palette="FancyPalettes.Witchlight" Duration="@TimeSpan.FromSeconds(12)">
    <section>Semantic content</section>
</GradientBackground>
```

`Angle` is clamped to `0..360`; `Duration` is nonnegative. `Animated` defaults to `true`; reduced motion retains the static gradient.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `FancyPalettes.Witchlight` | Four-color background palette. |
| `Angle` | `135` | Gradient angle, clamped to `0..360`. |
| `Animated`, `Duration` | `true`, `12 seconds` | CSS animation; reduced motion is static. |
| `Disabled` | `false` | Suppresses animation while retaining content. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor)

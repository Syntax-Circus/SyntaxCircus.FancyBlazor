# GradientText

`GradientText` applies a palette-derived CSS gradient to semantic child content.

```razor
<GradientText Palette="FancyPalettes.Glacier" Animated="true"><h2>Readable heading</h2></GradientText>
```

`Angle` is clamped to `0..360`; `Duration` is nonnegative. The gradient is static by default; set `Animated="true"` to slowly shift its colors. Reduced motion retains the static gradient.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `FancyPalettes.Witchlight` | Four-color text gradient. |
| `Angle` | `90` | Gradient angle. |
| `Animated`, `Duration` | `false`, `8 seconds` | Optional CSS animation. |
| `Disabled` | `false` | Restores inherited text color. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)

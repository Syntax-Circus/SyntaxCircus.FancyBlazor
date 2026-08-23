# Magnetic

`Magnetic` gives semantic content a subtle pointer-relative offset without intercepting clicks, focus, or keyboard interaction.

```razor
<Magnetic Strength=".2"><a href="/details">Open details</a></Magnetic>
```

`Strength` is clamped to `0..1`; `ResetDuration` is nonnegative. Reduced motion disables transforms.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Strength` | `.2` | Transform strength, clamped to `0..1`. |
| `ResetDuration` | `250 ms` | Nonnegative pointer-leave transition. |
| `Disabled` | `false` | Removes pointer tracking and resets the transform. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor)

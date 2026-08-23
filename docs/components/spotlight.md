# Spotlight

`Spotlight` places an `aria-hidden`, pointer-transparent radial light behind semantic content.

```razor
<Spotlight Color="#a7f3d0" Size="360" Opacity=".3"><article>Content</article></Spotlight>
```

`Size` is clamped to `32..1200` CSS pixels and `Opacity` to `0..1`. It remains centered when motion is reduced.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Host-supported CSS color. |
| `Size` | `320` | Diameter in CSS pixels, clamped to `32..1200`. |
| `Opacity` | `.25` | Decorative light opacity, clamped to `0..1`. |
| `Disabled` | `false` | Removes runtime pointer tracking and light. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor)

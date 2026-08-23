# NoiseOverlay

`NoiseOverlay` adds a non-interactive CSS grain layer behind semantic content.

```razor
<NoiseOverlay Opacity=".12"><article>Semantic content</article></NoiseOverlay>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Grain color. |
| `Opacity` | `.08` | Grain strength, clamped to `0..1`. |
| `Disabled` | `false` | Removes the decorative layer. |

The effect is intentionally static and requires no JavaScript. It adds texture immediately; it does not drift or animate. It safely becomes a plain wrapper where a browser does not support its blend styling.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)

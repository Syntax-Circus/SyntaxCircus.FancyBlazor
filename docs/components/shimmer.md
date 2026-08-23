# Shimmer

`Shimmer` overlays an `aria-hidden`, pointer-transparent decorative highlight sweep.

```razor
<Shimmer Color="currentColor" Intensity=".2"><article>Content</article></Shimmer>
```

`Intensity` is clamped to `0..1`; `Duration` is nonnegative. Reduced motion keeps a static highlight.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Host-supported CSS color. |
| `Intensity` | `.2` | Highlight opacity, clamped to `0..1`. |
| `Animated`, `Duration` | `true`, `2 seconds` | CSS sweep cycle. |
| `Disabled` | `false` | Hides the decorative layer. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor)

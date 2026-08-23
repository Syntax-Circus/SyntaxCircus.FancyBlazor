# EdgeGlow

`EdgeGlow` places one decorative CSS glow along an edge of existing content. It does not intercept pointer input or create focus behavior.

```razor
<EdgeGlow Color="#67e8f9" Placement="EdgeGlowPlacement.Top"><article>Featured content</article></EdgeGlow>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Glow color. |
| `Placement` | `Top` | `Top`, `Bottom`, `Start`, or `End`. |
| `Intensity` | `.45` | Glow strength, clamped to `0..1`. |
| `Size` | `28` | Glow extent in CSS pixels, clamped to `4..96`. |
| `Disabled` | `false` | Removes the glow. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

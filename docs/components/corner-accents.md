# CornerAccents

`CornerAccents` adds decorative opposing corners around existing content. The corners are pointer-transparent and hidden from assistive technology.

```razor
<CornerAccents Color="#a7f3d0"><article>Featured content</article></CornerAccents>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Accent color. |
| `Length` | `24` | Corner length in CSS pixels, clamped to `8..96`. |
| `Thickness` | `1` | Border thickness, clamped to `1..8`. |
| `Opacity` | `.7` | Accent opacity, clamped to `0..1`. |
| `Disabled` | `false` | Removes the corner decoration. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

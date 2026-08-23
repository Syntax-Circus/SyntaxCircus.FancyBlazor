# DotPattern

`DotPattern` renders a static dot field behind semantic content. Use it for quieter texture than a line grid.

```razor
<DotPattern Color="#c4b5fd" Spacing="20" DotSize="2">
    <section>Semantic content</section>
</DotPattern>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Host-supported CSS color for dots. |
| `Spacing` | `20` | Dot spacing in CSS pixels, clamped to `8..96`. |
| `DotSize` | `1` | Dot radius in CSS pixels, clamped to `1..8`. |
| `Opacity` | `.15` | Dot strength, clamped to `0..1`. |
| `Faded` | `true` | Applies a soft radial fade to the field. |
| `Disabled` | `false` | Removes the decorative field. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)

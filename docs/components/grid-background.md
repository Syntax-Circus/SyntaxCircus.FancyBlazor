# GridBackground

`GridBackground` renders a static line grid behind semantic content. It is a CSS-only structural field, not a canvas or interaction effect.

```razor
<GridBackground Color="#67e8f9" CellSize="32" Opacity=".12">
    <section>Semantic content</section>
</GridBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Host-supported CSS color for grid lines. |
| `CellSize` | `32` | Cell size in CSS pixels, clamped to `8..128`. |
| `Opacity` | `.12` | Grid strength, clamped to `0..1`. |
| `Faded` | `true` | Applies a soft radial fade to the field. |
| `Disabled` | `false` | Removes the decorative grid. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)

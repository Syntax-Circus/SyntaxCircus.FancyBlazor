# BorderBeam

`BorderBeam` places one moving, luminous segment around existing content. Use it when a focal surface needs a precise edge accent rather than the broader bloom of `GlowBorder`.

```razor
<BorderBeam Color="#a7f3d0" Duration="@TimeSpan.FromSeconds(4)">
    <article>Existing content</article>
</BorderBeam>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Host-supported CSS color for the beam. |
| `Thickness` | `1` | Edge thickness in CSS pixels, clamped to `1..8`. |
| `Duration` | `4 seconds` | One orbit; negative values become zero. |
| `Animated` | `true` | Enables the CSS orbit. Reduced motion is static. |
| `Disabled` | `false` | Removes the decorative edge. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)

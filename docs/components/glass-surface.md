# GlassSurface

`GlassSurface` adds a translucent reading plane around semantic content. It uses browser backdrop blur when available and retains its tint and border when it is not.

```razor
<GlassSurface Tint="rgba(7,17,31,.66)" Blur="20">
    <article>Readable content</article>
</GlassSurface>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Tint` | `rgba(255,255,255,.08)` | Host-supported CSS color for the surface. |
| `Blur` | `16` | Backdrop blur in CSS pixels, clamped to `0..64`. |
| `BorderOpacity` | `.18` | Current-color border strength, clamped to `0..1`. |
| `Disabled` | `false` | Removes the tint, border, and blur without removing content. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)

# Spatial surfaces

Spatial effects create layers behind or around content; they do not create application structure. Start with one field (`GridBackground` or `DotPattern`), add `OrbitalGlow` only when ambient movement helps, then use `GlassSurface` as the reading plane and `BorderBeam` as an optional focal accent.

```razor
<GridBackground>
    <OrbitalGlow>
        <GlassSurface>
            <BorderBeam><article>Semantic content</article></BorderBeam>
        </GlassSurface>
    </OrbitalGlow>
</GridBackground>
```

`GlassSurface` uses `backdrop-filter` when supported. Browsers without it retain the tint and border, so choose a `Tint` that keeps the content readable on its own. Grid and dot patterns are static; `BorderBeam` and `OrbitalGlow` hold a useful visual state under reduced motion.

The effects own no typography, spacing, corner radius, or contrast policy. Use `CssClass`, `Style`, and documented `--sc-fancy-*` variables for host styling, and verify foreground contrast in the composed surface.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)

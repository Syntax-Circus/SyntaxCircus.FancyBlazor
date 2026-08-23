# Palettes and Styling

Built-in palettes are `Midnight`, `Witchlight`, `Ember`, `Glacier`, and
`Viridian`. Create a custom palette with four CSS colors:

```csharp
var brand = new FancyPalette("#3b82f6", "#a855f7", "#22d3ee", "#07111f");
```

FancyBlazor ships only structural/effect styling. Use `CssClass`, `Style`, and
unmatched attributes for host integration. Stable outer classes begin with
`syntax-circus-fancy-`; documented variables begin with `--sc-fancy-`.

`GradientText` and `AuroraBackground` accept `FancyPalette`; their palette
variables follow the same primary, secondary, accent, and background mapping as
the existing gradient effects.
`OrbitalGlow` uses the same palette mapping. GlassSurface, BorderBeam,
GridBackground, and DotPattern use host CSS colors so they can follow an
existing surface or text color.

Do not target generated CSS-isolation attributes. Host CSS remains responsible
for typography, spacing, surfaces, focus, responsive layout, and contrast.

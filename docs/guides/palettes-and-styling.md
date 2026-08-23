# Palettes and Styling

Built-in palettes are `Midnight`, `Witchlight`, `Ember`, `Glacier`, and
`Viridian`. Create a custom palette with four CSS colors:

```csharp
var brand = new FancyPalette("#3b82f6", "#a855f7", "#22d3ee", "#07111f");
```

FancyBlazor ships only structural/effect styling. Use `CssClass`, `Style`, and
unmatched attributes for host integration. Stable outer classes begin with
`syntax-circus-fancy-`; documented variables begin with `--sc-fancy-`.

Do not target generated CSS-isolation attributes. Host CSS remains responsible
for typography, spacing, surfaces, focus, responsive layout, and contrast.

# Composition and authoring

FancyBlazor effects wrap consumer-owned semantic content. Start with one visual role, use a named preset when it matches that role, and add further layers only when they make the content easier to read or recognize.

| Preset | Decorative stack | Use it for |
| --- | --- | --- |
| `AuroraHero` | Aurora + grain | An ambient opening field. |
| `ReadingSurface` | Grid + orbital glow + glass + static beam | A protected reading plane. |
| `ActionCard` | Hover lift + press + focus halo | Existing interactive controls. |
| `EditorialHero` | Paper + text highlight | A warm editorial interruption. |
| `FeaturePanel` | Corners + edge glow | A constrained focal panel. |

All presets accept `Disabled`, `CssClass`, `Style`, unmatched attributes, and semantic child content. Palette-based presets also accept `Palette`; preset-specific presentation controls are listed by IntelliSense. Presets do not add roles, tab stops, click handlers, or application content.

| Preset | Additional typed parameters |
| --- | --- |
| `AuroraHero` | `Palette` (`FancyPalettes.Witchlight`), `Intensity` (`.55`, clamped to `0..1`) |
| `ReadingSurface` | `Palette` (`FancyPalettes.Viridian`), `Tint` (`rgba(7,17,31,.66)`) |
| `ActionCard` | `Color` (`currentColor`) |
| `EditorialHero` | `Tint` (`#fffaf0`), `Ink` (`#1f2937`), `HighlightColor` (`#fbbf24`) |
| `FeaturePanel` | `Color` (`currentColor`), `Placement` (`EdgeGlowPlacement.Top`), `Intensity` (`.45`, clamped to `0..1`) |

Keep native controls inside `ActionCard`, retain a real heading inside hero presets, and test the finished composition for contrast. For a separator that represents a document-level thematic break, use an ordinary `<hr>` in addition to—or instead of—the decorative divider components.

```razor
<ActionCard Color="#67e8f9">
    <button type="button">Save changes</button>
</ActionCard>
```

[Compiling examples](../../samples/FancyBlazor.Demo.Client/Pages/CompositionAuthoring.razor)

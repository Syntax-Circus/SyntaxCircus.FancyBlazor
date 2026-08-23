# PaperSurface

`PaperSurface` provides a tinted, lightly textured reading plane. Its texture is decorative; choose `Tint` and `Ink` with sufficient contrast for the composed content.

```razor
<PaperSurface Tint="#fffaf0" Ink="#1f2937"><article>Readable content</article></PaperSurface>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Tint` | `#fffaf0` | Surface background color. |
| `Ink` | `currentColor` | Foreground and border color. |
| `TextureOpacity` | `.08` | Grain opacity, clamped to `0..0.35`. |
| `BorderOpacity` | `.18` | Border strength, clamped to `0..1`. |
| `Disabled` | `false` | Removes the tint, border, and texture. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

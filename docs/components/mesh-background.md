# MeshBackground

`MeshBackground` adds a static palette-derived radial color field behind semantic content. It has no JavaScript lifecycle and leaves the palette background as its fallback.

```razor
<MeshBackground Palette="FancyPalettes.Viridian"><article>Readable content</article></MeshBackground>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `FancyPalettes.Witchlight` | Four-color field source. |
| `Intensity` | `.45` | Gradient strength, clamped to `0..1`. |
| `Disabled` | `false` | Keeps the palette background while removing the mesh. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

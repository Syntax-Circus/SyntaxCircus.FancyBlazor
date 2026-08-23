# ScrollBackdrop

`ScrollBackdrop` places a palette-derived decorative field behind ordinary content and shifts it with local scroll progress.

```razor
<ScrollBackdrop Palette="FancyPalettes.Viridian" Intensity=".42">
    <article>Readable content</article>
</ScrollBackdrop>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `FancyPalettes.Witchlight` | Four colors used by the decorative field. |
| `Intensity` | `.25` | Layer opacity, clamped to `0..1`. |
| `Disabled` | `false` | Leaves the semantic content and removes the enhancement. |

The backdrop layer is `aria-hidden` and pointer-transparent. Reduced motion holds it at a useful static state.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/NarrativeMotion.razor)

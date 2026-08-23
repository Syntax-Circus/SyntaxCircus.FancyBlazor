# Parallax

`Parallax` offsets content vertically as the wrapper crosses the viewport.

```razor
<Parallax Distance="32"><section>Semantic content</section></Parallax>
```

`Distance` is clamped to `0..300` CSS pixels. SSR and reduced-motion states render unshifted content.

For a depth scene, layer two or three `Parallax` wrappers with increasing
`Distance` values (for example `36`, `96`, and `180`) around decorative and
semantic planes. Avoid applying it to every card in a scrolling list.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Distance` | `24` | Maximum vertical offset, clamped to `0..300` CSS pixels. |
| `Disabled` | `false` | Removes scroll listeners and restores static content. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor)

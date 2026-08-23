# ScrollScene

`ScrollScene` continuously enhances a semantic section as it moves through the viewport.

```razor
<ScrollScene Effect="ScrollSceneEffect.Lift" Strength=".42" Travel="72">
    <article>Semantic chapter content</article>
</ScrollScene>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Effect` | `Lift` | `Fade`, `Lift`, or `Blur` viewport treatment. |
| `Strength` | `.25` | Effect strength, clamped to `0..1`. |
| `Travel` | `48` | Lift distance in CSS pixels, clamped to `0..300`. |
| `Disabled` | `false` | Removes scroll enhancement and restores static content. |

The semantic content remains visible for SSR, disabled, and reduced-motion states. The documented `--sc-fancy-scroll-progress` property is normalized from `0` at viewport entry to `1` at exit.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/NarrativeMotion.razor)

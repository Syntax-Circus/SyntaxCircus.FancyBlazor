# ScrollIndicator

`ScrollIndicator` adds an `aria-hidden` local progress line around semantic content.

```razor
<ScrollIndicator Color="#a7f3d0" Thickness="2">
    <article>A readable local chapter</article>
</ScrollIndicator>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Decorative line color. |
| `Thickness` | `2` | Line thickness in CSS pixels, clamped to `1..12`. |
| `Disabled` | `false` | Removes runtime progress tracking. |

Reduced motion shows a complete static line. The indicator is decorative and does not announce progress to assistive technology.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/NarrativeMotion.razor)

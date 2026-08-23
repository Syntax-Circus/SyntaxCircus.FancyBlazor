# TextStroke

`TextStroke` adds a CSS text outline around semantic child content. It is best for short display text; its readable `currentColor` fill remains useful in browsers without text-stroke support.

```razor
<TextStroke Color="#67e8f9" Width="1.5"><h2>Outlined heading</h2></TextStroke>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Outline color. |
| `Fill` | `currentColor` | CSS fill color behind the outline. |
| `Width` | `1` | Stroke width in CSS pixels, clamped to `0..8`. |
| `Disabled` | `false` | Restores inherited text rendering. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

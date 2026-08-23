# GradientDivider

`GradientDivider` is a decorative gradient line. It is hidden from assistive technology; use a native `<hr>` separately when the document needs a semantic thematic break.

```razor
<GradientDivider Color="#67e8f9" Thickness="2" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `StartColor` / `EndColor` | `transparent` | Colors at each line edge. |
| `Color` | `currentColor` | Center color. |
| `Thickness` | `1` | CSS-pixel thickness, clamped to `1..8`. |
| `Opacity` | `.65` | Line opacity, clamped to `0..1`. |
| `Disabled` | `false` | Removes the decorative line. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

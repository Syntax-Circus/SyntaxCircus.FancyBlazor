# SectionDivider

`SectionDivider` is a centered decorative marker with short lines on each side. It is not a replacement for a semantic `<hr>`.

```razor
<SectionDivider Color="#c4b5fd" Inset="12" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Marker and line color. |
| `Thickness` | `1` | Line thickness in CSS pixels, clamped to `1..8`. |
| `Inset` | `0` | Horizontal inset in CSS pixels, clamped to `0..160`. |
| `Disabled` | `false` | Removes the decoration. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

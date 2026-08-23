# HighlightText

`HighlightText` places a soft CSS marker wash behind common inline and heading text while retaining the child’s semantic elements.

```razor
<HighlightText Color="#fbbf24"><h2>Marked semantic heading</h2></HighlightText>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `#fbbf24` | Marker color. |
| `Opacity` | `.45` | Marker strength, clamped to `0..1`. |
| `Angle` | `-2` | Marker angle in degrees, clamped to `-12..12`. |
| `Disabled` | `false` | Removes the marker wash. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

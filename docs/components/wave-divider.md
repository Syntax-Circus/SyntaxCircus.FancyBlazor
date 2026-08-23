# WaveDivider

`WaveDivider` uses a static CSS radial-gradient wave as a decorative section transition. It adds no semantic landmark or announced separator.

```razor
<WaveDivider Color="#a7f3d0" Amplitude="10" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Wave color. |
| `Amplitude` | `8` | Wave height in CSS pixels, clamped to `2..32`. |
| `Thickness` | `2` | Wave band thickness, clamped to `1..8`. |
| `Opacity` | `.5` | Wave opacity, clamped to `0..1`. |
| `Disabled` | `false` | Removes the decoration. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

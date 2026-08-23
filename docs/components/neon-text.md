# NeonText

`NeonText` is a CSS-first glow and optional outline treatment for semantic child text.

```razor
<NeonText Color="#a7f3d0" Glow="10"><h2>Semantic heading</h2></NeonText>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Glow and optional outline color. |
| `Glow` | `8` | Glow radius in CSS pixels, clamped to `0..24`. |
| `StrokeWidth` | `0` | Optional outline width in CSS pixels, clamped to `0..4`. |
| `Disabled` | `false` | Removes the treatment. |

It uses no JavaScript, animation loop, or decorative replacement text. Check final foreground/background contrast in the host composition.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)

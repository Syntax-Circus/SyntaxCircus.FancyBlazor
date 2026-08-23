# StatusPulse

`StatusPulse` adds a small decorative pulse badge around a consumer-owned child control or panel.

```razor
<StatusPulse Color="#fbbf24"><button type="button">Save</button></StatusPulse>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Pulse color. |
| `Size` | `18` | Pulse diameter in CSS pixels, clamped to `4..48`. |
| `Animated` | `true` | Enables the decorative CSS pulse. |
| `Disabled` | `false` | Removes the layer. |

The wrapper creates no role, tab stop, click behavior, or status announcement. Its layer is decorative and `aria-hidden`; provide semantic status messaging separately when needed.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)

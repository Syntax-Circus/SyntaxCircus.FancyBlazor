# FocusHalo

`FocusHalo` adds a decorative halo when a descendant receives focus by pointer, touch, or keyboard.

```razor
<FocusHalo Color="#67e8f9" Spread="5">
    <label>Search <input type="search" /></label>
</FocusHalo>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Halo color. |
| `Opacity` | `.32` | Halo opacity, clamped to `0..1`. |
| `Spread` | `4` | Halo offset in CSS pixels, clamped to `0..16`. |
| `Duration` | `150 ms` | Nonnegative transition duration. |
| `Disabled` | `false` | Suppresses the decorative halo. |

FocusHalo works especially well around inputs and other controls with a persistent focus state. It does not remove or replace the host browser's focus outline, and its halo is hidden from assistive technology.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/InteractionFeedback.razor)

# PressScale

`PressScale` adds a compact response while a child control is pressed by pointer, Enter, or Space.

```razor
<PressScale Scale=".96"><button type="button">Save changes</button></PressScale>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Scale` | `.98` | Press scale, clamped to `.9..1`. |
| `Duration` | `100 ms` | Nonnegative transition duration. |
| `Disabled` | `false` | Removes listeners and restores the static state. |

PressScale never prevents pointer events, keyboard events, clicks, or navigation. Reduced motion removes the scale treatment.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/InteractionFeedback.razor)

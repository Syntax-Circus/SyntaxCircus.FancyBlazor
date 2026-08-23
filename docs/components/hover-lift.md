# HoverLift

`HoverLift` subtly raises its content for fine mouse or pen pointers.

```razor
<HoverLift Distance="8" Scale="1.02"><a href="/details">Open details</a></HoverLift>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Distance` | `4` | Upward distance in CSS pixels, clamped to `0..32`. |
| `Scale` | `1.01` | Hover scale, clamped to `.95..1.1`. |
| `Duration` | `150 ms` | Nonnegative CSS transition duration. |
| `Disabled` | `false` | Suppresses the hover treatment. |

Touch pointers do not receive hover enhancement. Reduced motion keeps the content static.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/InteractionFeedback.razor)

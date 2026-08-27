# NumberTicker

`NumberTicker` animates a numeric display toward a target value while an always-correct, always-visible value stays accessible.

```razor
<NumberTicker Value="1234.5" Format="N1" Duration="TimeSpan.FromMilliseconds(1500)" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Value` | `0` | Target numeric value. |
| `Format` | `"0"` | A .NET numeric format string applied to the final displayed value. |
| `Duration` | `1200 ms` | Nonnegative count animation length. |
| `Once`, `ReplayToken`, `Disabled` | `true`, `0`, `false` | Observer lifecycle controls. |

The animated digits are `aria-hidden`; a visually hidden sibling always holds the exact final formatted value, so assistive technology never announces intermediate counting frames. Static SSR and reduced motion show only the final value.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CoreKineticCatalog.razor)

# Stagger

`Stagger` reveals direct element children in sequence when its wrapper enters the viewport.

```razor
<Stagger Delay="@TimeSpan.FromMilliseconds(80)"><h2>First</h2><p>Second</p></Stagger>
```

`Effect`, `Delay`, `Duration`, `Distance`, and `Once` mirror `Reveal`; text nodes are not staggered. Content remains accessible and reduced motion presents the final state.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Effect` | `FadeUp` | `Fade`, `FadeUp`, or `BlurUp`. |
| `Delay` | `80 ms` | Nonnegative delay between direct children. |
| `Duration`, `Distance` | `500 ms`, `16` | Nonnegative transition duration and `0..500` travel distance. |
| `Once` | `true` | Stops observing after the first viewport entry. |
| `ReplayToken` | `0` | Changes restart the sequence after a short visible reset. |
| `Disabled` | `false` | Renders direct children without observer behavior. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpandedEffects.razor)

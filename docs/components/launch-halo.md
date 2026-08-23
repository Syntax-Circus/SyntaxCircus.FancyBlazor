# LaunchHalo

`LaunchHalo` adds a decorative animated halo behind an existing child control or panel.

```razor
<LaunchHalo Color="#67e8f9"><a href="/next">Continue</a></LaunchHalo>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Halo color. |
| `Intensity` | `.5` | Halo opacity, clamped to `0..1`. |
| `Spread` | `18` | Halo extent in CSS pixels, clamped to `0..64`. |
| `Animated` | `true` | Enables the decorative CSS animation. |
| `Disabled` | `false` | Removes the layer. |

The wrapper preserves the child link/button semantics, activation, and native focus outline. Its halo is `aria-hidden`, pointer-transparent, and static under reduced motion.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)

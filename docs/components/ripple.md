# Ripple

`Ripple` adds a decorative, pointer-originated wave without taking over its child interaction.

```razor
<Ripple><button type="button">Save changes</button></Ripple>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Wave color. |
| `Opacity` | `.24` | Wave opacity, clamped to `0..1`. |
| `Duration` | `600 ms` | Nonnegative wave lifetime. |
| `Disabled` | `false` | Suppresses JavaScript enhancement. |

Ripple does not prevent pointer events or replace link/button behavior. Use it on
an in-place interaction when the wave should be seen; a navigation can leave the
page before its short decorative animation finishes. Reduced motion creates none.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)

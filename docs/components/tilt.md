# Tilt

`Tilt` applies pointer-relative perspective without making its wrapper focusable
or altering child controls.

```razor
<Tilt MaxAngle="8" Perspective="900" Scale="1.02" Glare>
    <article><a href="/details">Interactive content</a></article>
</Tilt>
```

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/TiltPage.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `MaxAngle` | `double` | `10` | Degrees, clamped to `0..45`. |
| `Perspective` | `double` | `800` | CSS pixels, clamped to `100..4000`. |
| `Scale` | `double` | `1` | Engaged scale, clamped to `0.8..1.25`. |
| `Glare` | `bool` | `false` | Renders an `aria-hidden` highlight. |
| `GlareOpacity` | `double` | `0.2` | Clamped to `0..1`. |
| `ResetDuration` | `TimeSpan` | `250 ms` | Nonnegative pointer-leave reset. |
| `Disabled` | `bool` | `false` | Removes listeners and resets transforms. |
| `CssClass`, `Style` | `string?` | `null` | Extend the neutral wrapper. |
| `ChildContent` | `RenderFragment` | required | Content receiving perspective. |
| unmatched attributes | — | — | Applied to the outer wrapper. |

Pointer listeners are passive and never cancel clicks. Reduced motion disables
the transform and glare while preserving child interaction.

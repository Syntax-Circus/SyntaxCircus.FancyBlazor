# GlowBorder

`GlowBorder` adds a scoped CSS glow around existing content.

```razor
<GlowBorder Color="currentColor"
            Intensity="0.6"
            Radius="20"
            Duration="@TimeSpan.FromSeconds(4)">
    <article>Host-styled content</article>
</GlowBorder>
```

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Border.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `Color` | `string` | `currentColor` | Any host-supported CSS color. |
| `Intensity` | `double` | `0.5` | Clamped to `0..1`. |
| `Radius` | `double` | `12` | CSS pixels, clamped to `0..999`. |
| `Duration` | `TimeSpan` | `3 seconds` | One rotation; negative becomes zero. |
| `Animated` | `bool` | `true` | Enables rotation. |
| `Disabled` | `bool` | `false` | Removes the decorative border/padding. |
| `CssClass`, `Style` | `string?` | `null` | Extend the outer wrapper. |
| `ChildContent` | `RenderFragment` | required | Content inside the border. |
| unmatched attributes | — | — | Applied to the outer wrapper. |

The pseudo-element never enters the accessibility tree. Reduced motion stops
rotation while retaining the static edge light.

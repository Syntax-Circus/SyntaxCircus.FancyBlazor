# Reveal

`Reveal` transitions content when at least ten percent enters the viewport.

```razor
<Reveal Effect="RevealEffect.BlurUp"
        Delay="@TimeSpan.FromMilliseconds(100)"
        Duration="@TimeSpan.FromMilliseconds(500)">
    <section>Content remains accessible before reveal.</section>
</Reveal>
```

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/RevealPage.razor)

| Parameter | Type | Default | Behavior |
| --- | --- | --- | --- |
| `Effect` | `RevealEffect` | `FadeUp` | `Fade`, `FadeUp`, `BlurUp`, or a directional slide: `SlideFromLeft`, `SlideFromRight`, `SlideFromTop`, `SlideFromBottom`. Slides travel `Distance` pixels while fading in; raise `Distance` (for example `48`) for a pronounced slide. |
| `Delay` | `TimeSpan` | zero | Nonnegative transition delay. |
| `Duration` | `TimeSpan` | `500 ms` | Nonnegative transition duration. |
| `Distance` | `double` | `16` | CSS pixels, clamped to `0..500`. |
| `Once` | `bool` | `true` | Remains visible after first intersection. |
| `Disabled` | `bool` | `false` | Renders with no observer or transition. |
| `CssClass`, `Style` | `string?` | `null` | Extend the neutral wrapper. |
| `ChildContent` | `RenderFragment` | required | Content to reveal. |
| unmatched attributes | — | — | Applied to the outer wrapper. |

Reveal changes opacity/filter/transform only; it never applies `aria-hidden`.
Reduced motion immediately presents the final visible state.

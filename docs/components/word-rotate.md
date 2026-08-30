# WordRotate

`WordRotate` cycles a list of words with a transition between each, keeping the visible motion decorative and exposing a complete accessible mirror of the current word.

```razor
<WordRotate Words="@(new[] { "Compose", "Animate", "Ship" })" Interval="TimeSpan.FromSeconds(1.5)" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Words` | required | Two or more words. Validated at component initialization. |
| `Interval` | `2.5 s` | Clamped to `[250 ms, 30 s]`. The visible beat between words. |
| `Loop` | `true` | Re-cycle from the start after the last word. |
| `StartIndex` | `0` | Initial word index. |
| `Transition` | `Fade` | `Fade`, `SlideUp`, `SlideDown`, or `Blur`. |
| `Easing` | `ease-out` | Any CSS easing token. |
| `CssClass`, `Style`, `ChildContent`, `AdditionalAttributes` | n/a | Standard merging without dropping the stable `syntax-circus-fancy-word-rotate` hook. |
| `Disabled` | `false` | Settles to the first word and short-circuits the runtime. |

The host element renders as an inline `<span>` and gains the `syntax-circus-fancy-kinetic-text--static` class when `Disabled` is `true`. The visible motion respects `prefers-reduced-motion`: a reduced-motion user sees the first word and no transitions. Screen readers receive a `polite` `aria-live` mirror of the current word.

The runtime is destroyed on disposal; observers, request animation frames, and timeouts are released.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor)

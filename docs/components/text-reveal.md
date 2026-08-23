# TextReveal

`TextReveal` enhances required plain text into a word- or character-level viewport entrance while retaining the selected semantic text element.

```razor
<TextReveal Element="TextRevealElement.Heading1" Unit="TextRevealUnit.Word" Text="Semantic animated heading" />
```

Static SSR renders complete text. After interactivity, visual tokens are hidden from assistive technology and the semantic element retains the complete accessible name. Reduced motion and `Disabled` show the final text immediately.

The entrance starts when the element enters the viewport, not merely when the component renders. Use `ReplayToken` when a demo or application needs to restart the sequence.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Text` | required | Plain text to render and reveal. |
| `Element` | `Span` | `Span`, `Paragraph`, or `Heading1` through `Heading6`. |
| `Unit`, `Effect` | `Word`, `FadeUp` | Token size and transition. |
| `Delay`, `Stagger`, `Duration` | `0`, `70 ms`, `500 ms` | Nonnegative timing values. |
| `Distance` | `16` | Travel distance, clamped to `0..500`. |
| `Once`, `ReplayToken`, `Disabled` | `true`, `0`, `false` | Observer lifecycle controls. |

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)

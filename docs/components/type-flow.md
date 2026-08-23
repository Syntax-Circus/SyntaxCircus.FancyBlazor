# TypeFlow

`TypeFlow` reveals plain text by word or character as it enters the viewport while retaining one complete accessible text value.

```razor
<TypeFlow Element="TypeFlowElement.Paragraph" Text="Text that arrives with restraint." Unit="TextRevealUnit.Word" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Text` | required | Plain text to render. |
| `Element` | `Paragraph` | Semantic paragraph or heading element. |
| `Unit` | `Word` | `Word` or `Character` visual tokens. |
| `Direction` | `Up` | `Up` or `Down` visual entrance direction. |
| `Delay`, `Stagger`, `Duration` | `0`, `65 ms`, `500 ms` | Nonnegative timing values. |
| `Distance` | `16` | Travel distance, clamped to `0..500`. |
| `Once`, `ReplayToken`, `Disabled` | `true`, `0`, `false` | Observer lifecycle controls. |

Static SSR and reduced motion show the complete final text. After interactivity, visual tokens are decorative and the host element retains the complete accessible text name.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)

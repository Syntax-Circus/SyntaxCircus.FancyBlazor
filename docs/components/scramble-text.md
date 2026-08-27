# ScrambleText

`ScrambleText` reveals plain text through a per-character glyph-scramble animation while retaining one complete accessible text value.

```razor
<ScrambleText Element="TypeFlowElement.Heading2" Text="Decoded on arrival." />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Text` | required | Plain text to render. |
| `Element` | `Paragraph` | Semantic paragraph or heading element. |
| `Duration` | `600 ms` | Nonnegative per-character scramble duration. |
| `Stagger` | `24 ms` | Nonnegative delay added per character. |
| `Once`, `ReplayToken`, `Disabled` | `true`, `0`, `false` | Observer lifecycle controls. |

Static SSR and reduced motion show the complete final text. After interactivity, visual character tokens are decorative and the host element retains the complete accessible text name for the duration of the scramble.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CoreKineticCatalog.razor)

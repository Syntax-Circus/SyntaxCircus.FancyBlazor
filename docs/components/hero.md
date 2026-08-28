# Hero

`Hero` renders a themed, non-interactive `<div>` for a page or section
introduction, with an optional decorative background layer.

```razor
<Hero Alignment="HeroAlignment.Center">
    <Heading><h1>Composable effects for Blazor.</h1></Heading>
    <Subheading>Ship expressive UI with ordinary semantic HTML at the center.</Subheading>
    <Actions><FancyLink Href="/ui-companion">Get started</FancyLink></Actions>
</Hero>
```

`Heading`, `Subheading`, `Actions`, and `Background` are named `RenderFragment`
parameters, all optional. `Hero` does not choose a heading level for you, and
does not require or reference any WebGL or core renderer internals — the
optional `Background` slot can host any consumer-supplied content, including
a core visual effect, but `Hero` never requires one. `Background` renders
`aria-hidden` and pointer-transparent, layered behind the content.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Heading` | — | Optional heading content. Omitted from the DOM when not provided. |
| `Subheading` | — | Optional supporting copy. Omitted from the DOM when not provided. |
| `Actions` | — | Optional action content, typically `FancyButton`/`FancyLink`. Omitted from the DOM when not provided. |
| `Background` | — | Optional decorative background content, rendered `aria-hidden` and pointer-transparent. Omitted from the DOM when not provided. |
| `Alignment` | `HeroAlignment.Start` | `Start` for left-aligned content, or `Center` to center content. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

# FancyCard

`FancyCard` renders a themed, non-interactive `<article>` content surface
with optional header and footer slots.

```razor
<FancyCard>
    <Header>Plan: Pro</Header>
    <ChildContent><p>Everything in Free, plus priority support.</p></ChildContent>
    <Footer><FancyButton>Choose plan</FancyButton></Footer>
</FancyCard>
```

`Header` and `Footer` are named `RenderFragment` parameters; because `FancyCard`
declares them alongside `ChildContent`, wrap the body in an explicit
`<ChildContent>` tag whenever you also set `Header` or `Footer` (ordinary
Razor child-content rules — omit `Header`/`Footer` entirely and the body can
stay unwrapped).

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Header` | — | Optional header content. Omitted from the DOM when not provided. |
| `ChildContent` | — | Body content (required). |
| `Footer` | — | Optional footer content. Omitted from the DOM when not provided. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/UiCompanion.razor)

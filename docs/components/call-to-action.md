# CallToAction

`CallToAction` renders a themed, non-interactive `<div>` for a heading,
supporting copy, and one or more actions.

```razor
<CallToAction Layout="CallToActionLayout.Stacked">
    <Heading><h2>Ready to try FancyBlazor?</h2></Heading>
    <ChildContent>Start building your first effect in minutes.</ChildContent>
    <Actions>
        <FancyLink Href="/getting-started">Get started</FancyLink>
    </Actions>
</CallToAction>
```

`Heading`, `ChildContent`, and `Actions` are named `RenderFragment`
parameters; because `CallToAction` declares them together, wrap the copy in
an explicit `<ChildContent>` tag whenever you also set `Heading` or
`Actions`. `CallToAction` does not choose a heading level for you — supply
the `<h2>`–`<h6>` that fits your page's outline.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Heading` | — | Optional heading content. Omitted from the DOM when not provided. |
| `ChildContent` | — | Optional supporting copy. Omitted from the DOM when not provided. |
| `Actions` | — | Optional action content, typically `FancyButton`/`FancyLink`. Omitted from the DOM when not provided. |
| `Layout` | `CallToActionLayout.Inline` | `Inline` places heading/copy and actions in a row on wide viewports; `Stacked` centers everything in a column. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

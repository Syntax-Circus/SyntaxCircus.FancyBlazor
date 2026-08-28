# FancyNavbar

`FancyNavbar` renders a themed `<nav>` landmark with brand, links, and
actions slots, laid out horizontally with no JavaScript. It has no built-in
mobile disclosure — keyboard navigation is native tab order across whatever
interactive elements you place in the slots.

```razor
<FancyNavbar AriaLabel="Site">
    <Brand><FancyLink Href="/">Acme</FancyLink></Brand>
    <Links>
        <FancyLink Href="/pricing">Pricing</FancyLink>
        <FancyLink Href="/docs">Docs</FancyLink>
    </Links>
    <Actions><FancyButton>Sign in</FancyButton></Actions>
</FancyNavbar>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `AriaLabel` | `"Primary"` | Accessible label for the `nav` landmark. |
| `Brand` | — | Optional brand/logo slot. Omitted from the DOM when not provided. |
| `Links` | — | Optional navigation links slot. Omitted from the DOM when not provided. |
| `Actions` | — | Optional trailing actions slot. Omitted from the DOM when not provided. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/UiCompanion.razor)

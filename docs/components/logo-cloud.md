# LogoCloud

`LogoCloud` renders a themed, non-interactive `<ul>` list for a row of
consumer-owned partner or customer logos.

```razor
<LogoCloud AriaLabel="Trusted by">
    <li><img src="/logos/acme.svg" alt="Acme" /></li>
    <li><img src="/logos/globex.svg" alt="Globex" /></li>
    <li><img src="/logos/initech.svg" alt="Initech" /></li>
</LogoCloud>
```

`LogoCloud` lays out consumer-supplied `<li>` content; it does not fetch,
generate, or own any logo images, brand names, or links.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `ChildContent` | — | `<li>` content supplied by the consumer (required). |
| `AriaLabel` | `null` | Optional accessible label for the list; omitted from the DOM when not set. |
| `Layout` | `LogoCloudLayout.Wrap` | `Wrap` for comfortable spacing that wraps onto more rows, or `Dense` for tighter spacing. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

# PricingTable

`PricingTable` renders a themed, non-interactive `<table>` for comparing
consumer-owned plans and features.

```razor
<PricingTable AriaLabel="Plans">
    <thead>
        <tr>
            <th scope="col">Feature</th>
            <th scope="col">Free</th>
            <th scope="col" aria-current="true">Pro</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <th scope="row">Projects</th>
            <td>3</td>
            <td>Unlimited</td>
        </tr>
        <tr>
            <th scope="row">Support</th>
            <td>Community</td>
            <td>Priority</td>
        </tr>
    </tbody>
    <tfoot>
        <tr>
            <td></td>
            <td><FancyButton>Choose Free</FancyButton></td>
            <td><FancyButton>Choose Pro</FancyButton></td>
        </tr>
    </tfoot>
</PricingTable>
```

`PricingTable` lays out consumer-supplied `<thead>`/`<tbody>`/`<tfoot>`
markup; it does not compute plan counts, feature comparisons, or copy. Use
`scope="col"`/`scope="row"` on plan and feature headers for accessible table
semantics, and set `aria-current="true"` on a plan's header/cells yourself to
mark a featured tier — omit it entirely and the table works the same with no
featured tier.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `ChildContent` | — | `<thead>`/`<tbody>`/`<tfoot>` content supplied by the consumer (required). |
| `AriaLabel` | `null` | Optional accessible label for the table; omitted from the DOM when not set. |
| `Density` | `PricingTableDensity.Comfortable` | `Comfortable` or `Compact` cell padding. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

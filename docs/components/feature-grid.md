# FeatureGrid

`FeatureGrid` renders a themed, non-interactive `<ul>` responsive grid for a
set of consumer-owned feature callouts.

```razor
<FeatureGrid AriaLabel="Features" Columns="FeatureGridColumns.Three">
    <li>
        <h3>Fast</h3>
        <p>CSS-first effects with no required JavaScript.</p>
    </li>
    <li>
        <h3>Accessible</h3>
        <p>Reduced motion and semantic content by default.</p>
    </li>
    <li>
        <h3>Themeable</h3>
        <p>Typed tokens, no framework CSS required.</p>
    </li>
</FeatureGrid>
```

`FeatureGrid` lays out consumer-supplied `<li>` content (icon, heading, body
— composed however the consumer chooses); it does not own feature copy,
icons, or a typed item model.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `ChildContent` | — | `<li>` content supplied by the consumer (required). |
| `AriaLabel` | `null` | Optional accessible label for the list; omitted from the DOM when not set. |
| `Columns` | `FeatureGridColumns.Three` | Target column count (`Two`, `Three`, `Four`) on wide viewports; collapses responsively on narrower viewports. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

# FancyLink

`FancyLink` renders a native `<a>` with FancyBlazor UI theming.

```razor
<FancyLink Href="/details">Open details</FancyLink>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Href` | — | Link destination. Omitted from the rendered markup while `Disabled`. |
| `Target` | — | Native `target` attribute. `_blank` automatically adds `rel="noopener noreferrer"` unless you supply your own `rel`. |
| `Disabled` | `false` | Omits `href` (no default navigation, drops out of the tab order) and sets `aria-disabled="true"`. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/UiCompanion.razor)

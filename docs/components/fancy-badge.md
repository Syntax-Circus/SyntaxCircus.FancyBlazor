# FancyBadge

`FancyBadge` renders a themed, non-interactive `<span>` status label.

```razor
<FancyBadge>New</FancyBadge>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

`FancyBadge` has no `Disabled` parameter and adds no interaction of its own.

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/UiCompanion.razor)

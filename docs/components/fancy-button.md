# FancyButton

`FancyButton` renders a native `<button>` with FancyBlazor UI theming. Keyboard
operability (Enter/Space, tab order, native `disabled` behavior) comes from the
element itself.

```razor
<FancyButton Type="submit" OnClick="Save">Save changes</FancyButton>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Type` | `"button"` | Native `type` attribute: `"button"`, `"submit"`, or `"reset"`. |
| `Disabled` | `false` | Sets the native `disabled` attribute; suppresses `OnClick`. |
| `OnClick` | — | Invoked on click. Not raised while `Disabled`. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/UiCompanion.razor)

# Typewriter

`Typewriter` progressively types a list of lines character by character with an optional blinking caret and an optional delete phase, keeping the visible motion decorative and exposing the current full line to assistive technology.

```razor
<Typewriter Text="@(new[] { "Hello, world.", "Compose with FancyBlazor.", "Ship something fancy." })" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Text` | required | One or more lines. Validated at component initialization. |
| `Speed` | `60 ms` | Clamped to `[10 ms, 500 ms]`. Per-character add speed. |
| `HoldAfter` | `1.5 s` | Nonnegative hold after each line completes typing. |
| `DeleteSpeed` | `Speed` | `null` disables deletion; otherwise clamped to `[10 ms, 500 ms]`. |
| `Loop` | `true` | Re-cycle from the start after the last line. |
| `StartIndex` | `0` | Initial line index. |
| `Caret` | `true` | Whether to render the blinking caret. |
| `CaretCharacter` | `\|` | The character used for the caret. |
| `Direction` | `Auto` | `Auto`, `Ltr`, or `Rtl`. The host inherits the parent `direction`; this overrides. |
| `CssClass`, `Style`, `ChildContent`, `AdditionalAttributes` | n/a | Standard merging without dropping the stable `syntax-circus-fancy-typewriter` hook. |
| `Disabled` | `false` | Settles to the first line and short-circuits the runtime. |

The host element renders as an inline `<span>` and gains the `syntax-circus-fancy-kinetic-text--static` class when `Disabled` is `true`. The visible motion respects `prefers-reduced-motion`: a reduced-motion user sees the first line and no typing. The host sets `aria-live="off"` to prevent per-character screen-reader spam; the accessible text is updated only on full-line completion.

The caret blink is a CSS-only animation that is reduced-motion safe. The caret color reads `--sc-fancy-typewriter-caret-color` and falls back to `--sc-fancy-palette-accent` and then `currentColor`.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor)

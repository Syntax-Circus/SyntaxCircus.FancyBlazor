# MorphText

`MorphText` crossfades or character-splits between a list of strings with a visible hold between each, keeping the visible motion decorative and exposing a complete accessible mirror of the current word.

```razor
<MorphText Words="@(new[] { "Compose", "Animate", "Ship" })" Hold="TimeSpan.FromSeconds(1)" />
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Words` | required | Two or more words. Validated at component initialization. |
| `Duration` | `600 ms` | Clamped to `[100 ms, 2 s]`. Per-direction morph length. |
| `Hold` | `1.2 s` | Clamped to `[0, 10 s]`. Full-word hold between morphs. |
| `Loop` | `true` | Re-cycle from the start after the last word. |
| `StartIndex` | `0` | Initial word index. |
| `Mode` | `Crossfade` | `Crossfade` or `CharSplit`. `CharSplit` reads `--sc-fancy-palette-accent` for the highlighted word. |
| `Easing` | `cubic-bezier(0.22, 1, 0.36, 1)` | Any CSS easing token. |
| `CssClass`, `Style`, `ChildContent`, `AdditionalAttributes` | n/a | Standard merging without dropping the stable `syntax-circus-fancy-morph-text` hook. |
| `Disabled` | `false` | Settles to the first word and short-circuits the runtime. |

The host element renders as an inline `<span>` and gains the `syntax-circus-fancy-kinetic-text--static` class when `Disabled` is `true`. The visible motion respects `prefers-reduced-motion`: a reduced-motion user sees the first word and no transitions. Screen readers receive a `polite` `aria-live` mirror of the current word.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor)

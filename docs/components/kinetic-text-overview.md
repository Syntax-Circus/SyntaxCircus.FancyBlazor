# Kinetic text overview

The three kinetic text components — `WordRotate`, `MorphText`, and `Typewriter` — share a lifecycle but solve distinct decorative problems. Pick the one that matches the intent.

| When you want… | Use |
| --- | --- |
| A single cycling word that always occupies the same slot, like a hero or a tag line | [`WordRotate`](word-rotate.md) |
| Two or more complete phrases that swap with a visible hold and an optional character-split effect | [`MorphText`](morph-text.md) |
| A typewriter-style progressive reveal across one or more lines, with optional delete and an optional caret | [`Typewriter`](typewriter.md) |

All three:

- Render the visible motion as an `aria-hidden="true"` decorative layer.
- Expose a complete accessible text mirror for assistive technology (`polite` `aria-live` for `WordRotate` and `MorphText`; `off` for `Typewriter` to prevent per-character spam).
- Pause while offscreen via `IntersectionObserver`.
- Honor `prefers-reduced-motion: reduce` and the global `FancyMotionPreference` option.
- Settle to the first item and short-circuit the runtime when `Disabled` is `true`.
- Release every observer, request animation frame, and timer on disposal.
- Read palette custom properties (`--sc-fancy-palette-text`, `--sc-fancy-palette-accent`) and work without any palette set.

Compose them anywhere a normal text or heading would live, including inside `Hero`, `CallToAction`, and the rest of the UI companion.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/KineticTextShowcase.razor)

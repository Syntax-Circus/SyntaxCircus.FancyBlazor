# Accessibility

- Meaningful child content remains semantic DOM and retains its own headings,
  links, buttons, labels, and keyboard behavior.
- Shader canvases and glare are decorative: `aria-hidden`, non-focusable, and
  pointer-transparent.
- Wrappers add no roles or tab stops and do not suppress focus indicators.
- Reveal never removes content from the accessibility tree.
- `FancyMotionPreference.RespectSystem` is the default. Continuous motion becomes
  a static/final state when `prefers-reduced-motion: reduce` is active.
- `IgnoreSystem` is an explicit host decision; use it only when motion is
  essential and separately accessible.

The host remains responsible for contrast, accessible names, control semantics,
focus styles, and testing the final composition.

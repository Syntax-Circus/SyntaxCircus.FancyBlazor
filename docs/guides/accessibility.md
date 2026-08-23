# Accessibility

- Meaningful child content remains semantic DOM and retains its own headings,
  links, buttons, labels, and keyboard behavior.
- Shader canvases and glare are decorative: `aria-hidden`, non-focusable, and
  pointer-transparent.
- Spotlight and Shimmer layers are likewise decorative, `aria-hidden`, and
  pointer-transparent; GradientBackground remains a CSS-only backdrop.
- Wrappers add no roles or tab stops and do not suppress focus indicators.
- Reveal and Stagger never remove content from the accessibility tree.
- Tilt, Magnetic, and Parallax preserve child keyboard and pointer interaction;
  they resolve to their static state for reduced motion.
- Aurora, NoiseOverlay, Ripple, and CursorTrail layers are decorative,
  `aria-hidden`, and pointer-transparent. Ripple never replaces its child action.
- TextReveal keeps the requested heading, paragraph, or span semantic and exposes
  its complete text name; its visual word/character tokens are hidden from assistive technology.
- GridBackground, DotPattern, and OrbitalGlow layers are decorative, `aria-hidden`, and pointer-transparent. GlassSurface and BorderBeam add no role or tab stop and retain their child content unchanged.
- `FancyMotionPreference.RespectSystem` is the default. Continuous motion becomes
  a static/final state when `prefers-reduced-motion: reduce` is active.
- `IgnoreSystem` is an explicit host decision; use it only when motion is
  essential and separately accessible.

The host remains responsible for contrast, accessible names, control semantics,
focus styles, and testing the final composition.

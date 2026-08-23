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
- ScrollBackdrop and ScrollIndicator layers are decorative, `aria-hidden`, and pointer-transparent; ScrollScene never hides its semantic content. HoverLift and PressScale add no roles or tab stops. FocusHalo is decorative and supplements rather than removes the host focus outline.
- TextStroke and HighlightText retain their semantic child text. GradientDivider, WaveDivider, and SectionDivider are decorative and `aria-hidden`; use a native `<hr>` where a thematic break must be announced. MeshBackground, CornerAccents, PaperSurface texture, and EdgeGlow layers are decorative, pointer-transparent, and leave child semantics intact. Presets add no roles, tab stops, or activation behavior.
- ConstellationBackground and ArcFlowBackground canvases are decorative, `aria-hidden`, pointer-transparent, and hidden for reduced motion. NeonText retains semantic child text. TypeFlow retains a complete accessible text value while its visual tokens are decorative. StatusPulse and LaunchHalo add no roles, tab stops, click behavior, status announcement, or focus replacement; their layers are decorative only.
- `FancyMotionPreference.RespectSystem` is the default. Continuous motion becomes
  a static/final state when `prefers-reduced-motion: reduce` is active.
- `IgnoreSystem` is an explicit host decision; use it only when motion is
  essential and separately accessible.

The host remains responsible for contrast, accessible names, control semantics,
focus styles, and testing the final composition.

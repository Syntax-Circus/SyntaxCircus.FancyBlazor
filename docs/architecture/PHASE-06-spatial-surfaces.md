# PHASE-06: Spatial Surfaces

## Objective

Extend the preview catalog with composable CSS-first depth treatments without
adding a runtime dependency or changing consumer setup.

## Actionable tasks

- [x] **SP-01** Implement `GlassSurface`, `BorderBeam`, `GridBackground`, `DotPattern`, and `OrbitalGlow` with progressive CSS fallbacks.
- [x] **SP-02** Add a compiling layered spatial-surfaces demo and navigation.
- [x] **SP-03** Document every public API, styling variable, accessibility, performance, and fallback behavior.
- [x] **SP-04** Add bUnit and Playwright coverage, including reduced-motion visual evidence.

## Success criteria

Every effect preserves semantic content, emits only scoped styles, requires no
consumer JavaScript setup, and has a static useful state under reduced motion.

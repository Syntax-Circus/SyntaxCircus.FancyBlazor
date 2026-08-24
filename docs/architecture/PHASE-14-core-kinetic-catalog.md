# PHASE-14: Core Kinetic Catalog

## Objective

Expand the core package with reusable kinetic content accents and lightweight
atmospheric fields without adding widget semantics or another rendering engine.

## Committed components

- `Marquee`
- `NumberTicker`
- `ScrambleText`
- `LightRaysBackground`
- `MeteorBackground`
- `FlickerGrid`

## Actionable tasks

- [ ] Define typed parameters, stable hooks, CSS variables, clamping, and
  static/reduced-motion behavior for all six components.
- [ ] Preserve one meaningful semantic representation for moving or transformed
  content; visual duplicates and generated glyphs must be decorative and
  unfocusable.
- [ ] Bound Canvas/DOM work, pause it while hidden or offscreen, and release all
  frames, observers, listeners, and generated nodes on disposal.
- [ ] Add compiling demos, component guides, README catalog entries,
  accessibility/performance guidance, changelog notes, and contributor rules.
- [ ] Add bUnit and browser coverage for SSR/final states, semantics, motion
  preferences, visibility pausing, input clamping, and disposal.

## Success criteria

All six effects compose around ordinary content, retain useful static output,
introduce no widget behavior, and pass the full core release and package gates.

## Validation gate

Run the Release build and test suites, browser lifecycle coverage,
documentation verification, core packaging/content inspection, and clean core
package-consumer build.

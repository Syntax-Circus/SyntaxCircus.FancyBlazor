# PHASE-07: Narrative Motion

## Objective

Add `ScrollScene`, `ScrollIndicator`, and `ScrollBackdrop` as composable, normal-flow narrative effects with no scroll-jacking or consumer setup.

## Actionable tasks

- [x] Implement the three components, typed scene effect, scoped styles, and intersection-gated runtime lifecycle.
- [x] Add a compiling narrative-motion demo with exact source snippets and navigation.
- [x] Document public API, CSS properties, accessibility, performance, and reduced-motion behavior for users and agents.
- [x] Add bUnit and Playwright coverage for semantics, progress, hidden/offscreen cleanup, and disposal.

## Success criteria

Semantic content remains useful during SSR and reduced motion. Scroll work uses only passive event-batched progress while intersecting and has no active frame while hidden, offscreen, or disposed.

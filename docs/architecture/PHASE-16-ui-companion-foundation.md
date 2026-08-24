# PHASE-16: UI Companion Foundation

## Objective

Establish `SyntaxCircus.FancyBlazor.UI` as an optional, exact-version companion
for styled, accessible controls without moving widget semantics into core or
making WebGL transitive.

## Committed components

- `FancyButton`
- `FancyLink`
- `FancyBadge`
- `FancyCard`
- `FancyNavbar`

## Actionable tasks

- [ ] Add the `net10.0` UI RCL with public types in
  `SyntaxCircus.FancyBlazor`, `AddFancyBlazorUi()` registration, and an exact
  core dependency; do not reference the WebGL package.
- [ ] Define typed theme/token options, stable `syntax-circus-fancy-ui-*` hooks,
  `--sc-fancy-ui-*` custom properties, and scoped styles without global resets
  or host typography.
- [ ] Implement the five controls with native HTML semantics, complete keyboard
  behavior, consumer-provided content, attribute merging, visible focus, and
  reduced-motion-safe effects.
- [ ] Add a package README, compiling demo route, user guides, accessibility and
  styling guidance, changelog entries, and contributor contracts.
- [ ] Extend CI, release scripts, package inspection, and clean-consumer tests so
  core, WebGL, and UI publish as one exact-version release set.

## Success criteria

A clean consumer can install UI, call `AddFancyBlazorUi()`, import one namespace,
and use all five controls without WebGL, Node, npm, a CDN, manual scripts, global
CSS, or a project reference.

## Validation gate

Run Release build/tests, accessibility-focused bUnit and browser coverage,
documentation checks, all three package inspections, isolated core/UI clean
consumers, and the three-package same-version release-set verifier.

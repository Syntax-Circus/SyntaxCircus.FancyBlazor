# PHASE-15: WebGL Fields and Materials

## Objective

Complete the approved follow-up WebGL catalog using the existing optional
Three.js companion boundary and typed progressive enhancement.

## Committed components

- `ParticleFieldBackground`
- `WaveFieldBackground`
- `RefractiveOrbBackground`
- `PrismFieldBackground`

## Actionable tasks

- [ ] Implement all four components through the existing companion runtime with
  typed controls; keep renderer names, scene objects, raw uniforms, and GLSL
  internal.
- [ ] Retain semantic child content and useful CSS fallbacks during SSR, reduced
  motion, explicit disablement, missing WebGL, and renderer failure.
- [ ] Cap DPR and scene complexity, pause hidden/offscreen work, and dispose
  every frame, listener, observer, geometry, material, texture, and context.
- [ ] Extend the WebGL showcase, guides, package README, changelog, and
  accessibility/performance documentation with compiling examples.
- [ ] Add bUnit and Playwright coverage for creation, coarse updates, fallback,
  reduced motion, visibility pausing, repeated mount/unmount, and disposal.

## Success criteria

The companion offers five distinct typed WebGL effects without a new engine,
consumer JavaScript tooling, CDN assets, or leaked GPU/browser resources.

## Validation gate

Run the full Release, browser, documentation, core/companion pack, content,
clean-consumer, and same-version release-set gates. The adapter and renderer
must remain below the existing 1 MiB raw and 250 KiB Brotli limits.

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

### Shared prerequisite

- [x] Generalize `fancy-blazor-webgl.js`'s effect dispatch into a small
  per-effect registry (canvas class, renderer module path, factory export)
  used by both `createEffect` and `pump`; migrate `holographic-surface` onto
  it with no behavior change; confirm the full HolographicSurface bUnit and
  Playwright suites still pass before any new effect is registered.

### WaveFieldBackground

- [x] Implement `WaveFieldBackground` through the existing companion runtime
  with typed `Palette`, `Intensity`, `Speed`, `Interactive`, `Quality`,
  `Disabled`, `Amplitude`, `Frequency`, and `Foam` controls; keep the
  renderer name, scene objects, raw uniforms, and GLSL internal.
- [x] Retain semantic child content and a palette-derived static CSS fallback
  during SSR, reduced motion, explicit disablement, missing WebGL, and
  renderer failure.
- [x] Cap DPR by quality, pause hidden/offscreen work, and dispose every
  frame, listener, observer, geometry, material, and context on teardown.
- [x] Add a README usage section, `docs/components/wave-field-background.md`,
  a WebGL showcase example, and accessibility/performance documentation
  updates.
- [x] Add bUnit and Playwright coverage for creation, coarse updates,
  fallback, reduced motion, visibility pausing, repeated mount/unmount, and
  disposal.

### RefractiveOrbBackground

- [x] Implement `RefractiveOrbBackground` through the existing companion
  runtime with typed `Palette`, `Intensity`, `Speed`, `Interactive`,
  `Quality`, `Disabled`, `Radius`, `Distortion`, and `Sheen` controls; keep
  the renderer name, scene objects, raw uniforms, and GLSL internal.
- [x] Retain semantic child content and a palette-derived static CSS fallback
  during SSR, reduced motion, explicit disablement, missing WebGL, and
  renderer failure.
- [x] Cap DPR by quality, pause hidden/offscreen work, and dispose every
  frame, listener, observer, geometry, material, and context on teardown.
- [x] Add a README usage section,
  `docs/components/refractive-orb-background.md`, a WebGL showcase example,
  and accessibility/performance documentation updates.
- [x] Add bUnit and Playwright coverage for creation, coarse updates,
  fallback, reduced motion, visibility pausing, repeated mount/unmount, and
  disposal.

### PrismFieldBackground

- [x] Implement `PrismFieldBackground` through the existing companion runtime
  with typed `Palette`, `Intensity`, `Speed`, `Interactive`, `Quality`,
  `Disabled`, `Facets`, `Dispersion`, and `Sheen` controls; keep the renderer
  name, scene objects, raw uniforms, and GLSL internal.
- [x] Retain semantic child content and a palette-derived static CSS fallback
  during SSR, reduced motion, explicit disablement, missing WebGL, and
  renderer failure.
- [x] Cap DPR by quality, pause hidden/offscreen work, and dispose every
  frame, listener, observer, geometry, material, and context on teardown.
- [x] Add a README usage section, `docs/components/prism-field-background.md`,
  a WebGL showcase example, and accessibility/performance documentation
  updates.
- [x] Add bUnit and Playwright coverage for creation, coarse updates,
  fallback, reduced motion, visibility pausing, repeated mount/unmount, and
  disposal.

### ParticleFieldBackground

- [x] Implement `ParticleFieldBackground` through the existing companion
  runtime with a bounded GPU point-sprite field and typed `Palette`,
  `Intensity`, `Speed`, `Interactive`, `Quality`, `Disabled`, `Density`,
  `Size`, and `Drift` controls; keep the renderer name, scene objects, raw
  uniforms, and GLSL internal.
- [x] Retain semantic child content and a palette-derived static CSS fallback
  during SSR, reduced motion, explicit disablement, missing WebGL, and
  renderer failure.
- [x] Cap DPR and particle count by quality, pause hidden/offscreen work, and
  dispose every frame, listener, observer, geometry, material, and context on
  teardown.
- [x] Add a README usage section,
  `docs/components/particle-field-background.md`, a WebGL showcase example,
  and accessibility/performance documentation updates.
- [x] Add bUnit and Playwright coverage for creation, coarse updates,
  fallback, reduced motion, visibility pausing, repeated mount/unmount, and
  disposal.

### Package and release

- [x] Confirm each renderer's packaged path is present in
  `eng/verify-webgl-package.ps1`'s required-entries and owned-scripts lists,
  with the combined adapter/renderer payload below 1 MiB raw / 250 KiB
  Brotli. (45,300 bytes raw / 13,597 bytes Brotli combined across the
  adapter and all five renderers — well within budget.)
- [x] Run the full Release, browser, documentation, core/companion pack,
  content, clean-consumer, and same-version release-set gates with all five
  effects present. (125/125 tests passed; all package/doc verifiers passed.)

## Success criteria

The companion offers five distinct typed WebGL effects without a new engine,
consumer JavaScript tooling, CDN assets, or leaked GPU/browser resources.

## Validation gate

Run the full Release, browser, documentation, core/companion pack, content,
clean-consumer, and same-version release-set gates. The adapter and renderer
must remain below the existing 1 MiB raw and 250 KiB Brotli limits. Run the
targeted subset of this gate after each component's checkboxes are checked
off, and the full gate once all four components are complete.

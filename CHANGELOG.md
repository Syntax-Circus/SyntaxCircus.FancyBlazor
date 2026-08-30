# Changelog

All notable changes to `SyntaxCircus.FancyBlazor` are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Until 1.0, changes that alter public components, parameters, defaults, rendered
markup hooks, CSS custom properties, registration, or hosting behavior must be
called out clearly as potentially breaking.

## [Unreleased]

### Added

- Add `WordRotate` to the core package: cycles a list of headline words with a
  `Fade`/`SlideUp`/`SlideDown`/`Blur` transition while keeping the visible
  motion decorative and exposing a complete accessible text mirror.
- Add `MorphText` to the core package: crossfades or character-splits between
  strings with a typed `Mode` (`Crossfade`/`CharSplit`) and a visible hold
  between each.
- Add `Typewriter` to the core package: progressively types a list of lines
  with a typed `Speed`, optional `DeleteSpeed`, optional blinking caret, and
  optional `Direction` override.
- Add a new `KineticTextShowcase` route to the demo, linked from the primary
  nav bar, the footer nav, the home effect-grid, and the home catalog
  directory.
- Add the optional `SyntaxCircus.FancyBlazor.UI` companion package with typed
  `FancyUiTheme`/`FancyUiThemes` tokens and `AddFancyBlazorUi()` registration,
  depending only on core and never pulling in WebGL.
- Add `FancyButton` to `SyntaxCircus.FancyBlazor.UI`: a native `<button>`
  with typed theme tokens, native keyboard operability, and no JavaScript
  lifecycle.
- Add `FancyLink` to `SyntaxCircus.FancyBlazor.UI`: a native `<a>` with typed
  theme tokens, automatic `rel="noopener noreferrer"` for `target="_blank"`,
  and an `href`-omitting disabled state.
- Add `FancyBadge` to `SyntaxCircus.FancyBlazor.UI`: a themed, non-interactive
  `<span>` status label.
- Add `FancyCard` to `SyntaxCircus.FancyBlazor.UI`: a themed `<article>`
  content surface with optional header/footer slots that are omitted from
  the DOM when not provided.
- Add `FancyNavbar` to `SyntaxCircus.FancyBlazor.UI`: a themed `<nav>`
  landmark with optional brand/links/actions slots, laid out with no
  JavaScript and no built-in mobile disclosure.
- Add `LogoCloud` to `SyntaxCircus.FancyBlazor.UI`: a themed `<ul>` list for
  consumer-owned partner/customer logos with a typed `Layout` (`Wrap`/`Dense`)
  presentation option and no embedded logo content.
- Add `Testimonial` to `SyntaxCircus.FancyBlazor.UI`: a themed
  `<figure>`/`<blockquote>` for a single quote with optional attribution and
  avatar slots, omitted from the DOM when not provided.
- Add `CallToAction` to `SyntaxCircus.FancyBlazor.UI`: a themed heading/copy/
  actions block with a typed `Layout` (`Inline`/`Stacked`) presentation
  option; does not choose a heading level for consumers.
- Add `FeatureGrid` to `SyntaxCircus.FancyBlazor.UI`: a themed `<ul>`
  responsive grid for consumer-owned feature callouts with a typed `Columns`
  (`Two`/`Three`/`Four`) presentation option.
- Add `Hero` to `SyntaxCircus.FancyBlazor.UI`: a themed heading/subheading/
  actions block with an optional `aria-hidden`, pointer-transparent
  `Background` slot that never requires WebGL or core renderer internals.
- Add `PricingTable` to `SyntaxCircus.FancyBlazor.UI`: a themed `<table>` for
  consumer-owned plan/feature comparisons with a typed `Density`
  (`Comfortable`/`Compact`) presentation option; works with or without a
  featured tier.
- Add `FaqAccordion`/`FaqAccordionItem` to `SyntaxCircus.FancyBlazor.UI`: a
  themed, keyboard-operable disclosure list with a typed `SingleOpen`
  presentation option, an optional `Animated` CSS height-transition open/close
  that respects `prefers-reduced-motion`, and a full-width-by-default layout
  that stays stable inside a centering flex or grid parent. This is the first
  control in the UI companion package with a JavaScript lifecycle
  (`faq-accordion.js`): it owns click-driven open/closed state and listener
  cleanup, and never sends per-frame updates to Blazor. Every other UI
  companion control remains JavaScript-free.
- Verify (ADR-016) that every `SyntaxCircus.FancyBlazor.UI` control coexists
  cleanly with Bootstrap 5's Reboot: dedicated Playwright coverage renders
  the catalog with and without a locally vendored, unmodified Bootstrap 5
  stylesheet (test/demo-only; never packed) and asserts identical computed
  styles.

- Add `WaveFieldBackground` to `SyntaxCircus.FancyBlazor.WebGL`: a
  Three.js-backed interference wave field with typed `Amplitude`,
  `Frequency`, and `Foam` controls, reusing the vendored r184 build and the
  shared companion runtime's lifecycle, fallback, and disposal guarantees.
- Add `RefractiveOrbBackground` to `SyntaxCircus.FancyBlazor.WebGL`: a single
  analytically-lensed glass orb with typed `Radius`, `Distortion`, and
  `Sheen` controls, drawn in one shader pass with no textures or render
  targets.
- Add `PrismFieldBackground` to `SyntaxCircus.FancyBlazor.WebGL`: a
  procedurally faceted tiling with typed `Facets`, `Dispersion`, and `Sheen`
  controls, drawn in one shader pass with no mesh subdivision, textures, or
  render targets.
- Add `ParticleFieldBackground` to `SyntaxCircus.FancyBlazor.WebGL`, completing
  the four-effect Phase 15 catalog: a quality-tiered, bounded GPU point-sprite
  field with typed `Density`, `Size`, and `Drift` controls that rebuilds its
  point buffer in place when the resolved particle count changes.

- Add a five-phase pre-1.0 roadmap for core kinetic effects, four WebGL fields
  and materials, the optional `SyntaxCircus.FancyBlazor.UI` companion, its
  marketing/content catalog, and cross-package stabilization.
- Link the hosted FancyBlazor demo from the README.
- Publish the optional `SyntaxCircus.FancyBlazor.WebGL` preview companion at the
  exact core package version, with locally vendored Three.js r184 assets,
  package isolation, size budgets, clean-consumer proof, and a same-version
  release guard.
- Add a discoverable WebGL showcase with live typed controls, calibrated
  presets, fallback inspection, preview/setup guidance, ThreeUI inspiration
  attribution, and semantic-content preservation coverage.
- Add six core kinetic and atmospheric components: bounded Canvas 2D
  `FlickerGrid`, `MeteorBackground`, and `LightRaysBackground`; semantic
  `ScrambleText` character-scramble reveal; `Marquee` seamless looping
  content scroll; and `NumberTicker` animated count-up with an
  always-accessible final value.
- Add a compiling Core Kinetic Catalog demo route with each new component,
  plus component guides and accessibility/performance documentation.

### Fixed

- Recreate `HolographicSurface`'s decorative canvas after disabling its WebGL
  runtime so re-enabling cannot reuse a deliberately context-lost canvas.
- Wait for `TypeFlow`'s warm-up animation frame to settle before asserting
  zero active frames in the no-canvas-context browser test, instead of
  sampling once immediately after `data-fancy-ready` is set.
- Fix `Marquee`'s track shrinking inside a constrained flex/grid ancestor,
  which broke the seamless `-50%` loop and caused it to reset before
  scrolling the full duplicated content.

## [0.2.0] - 2026-08-23

### Added

- Add six pre-1.0 public components: bounded Canvas 2D `ConstellationBackground`
  and `ArcFlowBackground`; CSS-first `NeonText`, `StatusPulse`, and `LaunchHalo`;
  and semantic text entrance `TypeFlow`.
- Add a compiling Atmosphere and Accents demo route with each v0.2.0 component,
  plus component guides and accessibility/performance documentation.
- Add Canvas lifecycle, reduced-motion, semantic-content, static-fallback, and
  cleanup coverage for the v0.2.0 catalog.

### Changed

- Expand the pre-1.0 public API surface; consumers should treat all new
  components, parameters, stable hooks, and CSS custom properties as preview API
  subject to the repository's documented compatibility policy.

## [0.1.5] - 2026-08-23

### Added

- Add nine CSS-first typography, separator, and surface effects: `TextStroke`,
  `HighlightText`, `GradientDivider`, `WaveDivider`, `SectionDivider`,
  `MeshBackground`, `CornerAccents`, `PaperSurface`, and `EdgeGlow`.
- Add five typed composition presets: `AuroraHero`, `ReadingSurface`,
  `ActionCard`, `EditorialHero`, and `FeaturePanel`.
- Add compiling CSS-first Catalog and Composition and Authoring demo pages,
  documentation samples, component guides, and contributor guidance.
- Add a complete grouped footer sitemap plus `robots.txt` and `sitemap.xml` to
  the demo so every route is discoverable by visitors and compliant crawlers.
- Add the focused Phase 9 compatibility, fallback, public-API, and visual
  baseline release gate for the new catalog.

## [0.1.4] - 2026-08-23

### Added

- Add `ScrollScene`, `ScrollIndicator`, and `ScrollBackdrop` for semantic,
  normal-flow narrative motion with static reduced-motion fallbacks.
- Add `HoverLift`, `PressScale`, and `FocusHalo` for additive fine-pointer,
  pointer/keyboard, and keyboard-visible focus feedback.
- Add compiling Narrative Motion and Interaction Feedback demo pages with live
  examples and matching source snippets.
- Document both collections for consumers and contributors, including lifecycle,
  accessibility, device behavior, performance, and future roadmap work.
- Add component and browser lifecycle coverage for scroll, feedback, and
  accessibility behavior.

### Changed

- Make `FocusHalo` appear for pointer, touch, and keyboard focus while retaining
  the browser's native focus outline.

## [0.1.3] - 2026-08-22

### Added

- Add `GlassSurface`, `BorderBeam`, `GridBackground`, `DotPattern`, and
  `OrbitalGlow` to the FancyBlazor preview catalog.
- Add a documented, compiling spatial-surfaces demo and reduced-motion browser
  coverage for the CSS-first surface collection.

### Fixed

- Publish the Interactive Auto demo's `blazor.web.js` boot asset so it remains
  available through a standard reverse proxy such as Caddy.

## [0.1.2] - 2026-08-22

### Added

- Add `GradientText`, `TextReveal`, `AuroraBackground`, `NoiseOverlay`, `Ripple`, and `CursorTrail` to the FancyBlazor preview catalog.
- Publish the Interactive Auto demo as a public GHCR image from successful
  `main` builds, with `latest` and immutable commit-SHA tags.
- Document local image builds and deployment behind a user-managed reverse
  proxy.

### Fixed

- Make browser tests NCrunch-compatible by launching compiled test-host
  assemblies instead of searching for the source repository at runtime, and by
  excluding child-process assemblies from NCrunch instrumentation.
- Exclude Playwright browser integration tests from NCrunch while preserving
  their regular-runner and CI coverage.
- Ensure `Reveal` applies its entry state for one animation frame before it
  begins viewport observation, making the initial visual transition reliable.
- Remove the demo's distracting default browser outline from programmatically
  focused route headings while retaining `FocusOnNavigate` for accessibility.

### Changed

- Make the Reveal demo's sequence longer, staggered, and replayable so its
  viewport behavior is observable rather than easy to miss on initial load.

## [0.1.1] - 2026-08-22

### Added

- Add `GradientBackground`, `Spotlight`, `Magnetic`, `Parallax`, `Stagger`, and
  `Shimmer` to the FancyBlazor preview catalog.

## [0.1.0-preview.1] - 2026-08-22

### Added

- Initial `net10.0` Razor Class Library package: `SyntaxCircus.FancyBlazor`.
- `ShaderBackground` with vendored Nacre WebGL shader, palette-derived CSS
  fallback, reduced-motion support, quality caps, hidden/offscreen pausing, and
  automatic failure fallback.
- CSS-first `GlowBorder`, observer-driven `Reveal`, and pointer-driven `Tilt`
  components, designed to compose around ordinary semantic Blazor content.
- Typed `AddFancyBlazor()` registration, global motion/quality/pause options,
  built-in palettes, and `TimeSpan`-based effect timings.
- Support for static SSR, Interactive Server, Interactive WebAssembly,
  Interactive Auto, and standalone WebAssembly consumers.
- Interactive Auto demo, standalone package consumer, component examples, user
  guides, contributor/agent contract, and provenance/third-party notices.
- xUnit, bUnit, Playwright, deterministic visual-artifact, package-content,
  documentation-link, and clean-package-consumer validation.
- GitVersion-based CI, NuGet Trusted Publishing, package artifact upload, and
  release tagging workflow.

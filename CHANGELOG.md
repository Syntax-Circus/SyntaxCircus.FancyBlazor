# Changelog

All notable changes to `SyntaxCircus.FancyBlazor` are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Until 1.0, changes that alter public components, parameters, defaults, rendered
markup hooks, CSS custom properties, registration, or hosting behavior must be
called out clearly as potentially breaking.

## [Unreleased]

### Added

- Validate an unpublished `SyntaxCircus.FancyBlazor.WebGL` companion boundary
  spike locally and in CI, with locally vendored Three.js r184 assets, package
  isolation, size budgets, and a clean consumer proof. This is not a published
  package or part of the advertised FancyBlazor catalog.

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

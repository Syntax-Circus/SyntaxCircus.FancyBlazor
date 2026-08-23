# Changelog

All notable changes to `SyntaxCircus.FancyBlazor` are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Until 1.0, changes that alter public components, parameters, defaults, rendered
markup hooks, CSS custom properties, registration, or hosting behavior must be
called out clearly as potentially breaking.

## [Unreleased]

Use this section for changes merged after the latest published package. Keep
entries grouped under `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, or
`Security`; move them to a dated version section during release preparation.

### Added

- Add `GradientBackground`, `Spotlight`, `Magnetic`, `Parallax`, `Stagger`, and
  `Shimmer` to the FancyBlazor preview catalog.

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

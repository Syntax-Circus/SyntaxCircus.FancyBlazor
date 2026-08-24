# Phase 13: WebGL Rendering Boundary

## Outcome

Phase 13 establishes `SyntaxCircus.FancyBlazor.WebGL` as an optional preview
Razor Class Library companion. ADR-013 selects this boundary instead of an
opt-in WebGL mode inside `SyntaxCircus.FancyBlazor`; ADR-014 promotes the
validated package to same-version NuGet publication.

The core package and its runtime remain unchanged. The companion owns its
registration (`AddFancyBlazorWebGl`), JavaScript lifecycle, CSS isolation, and
vendored Three.js r184 ESM assets. A clean Razor consumer restores the local
core and companion packages, registers the companion, and compiles without a
project reference, Node, npm, CDN, or a manual script import. The release guard
requires the core and companion package versions to match exactly.

## Why a companion package

Keeping the renderer boundary separate prevents a Three.js payload and its GPU
lifecycle from becoming an opt-in-but-shipped cost of the core package. It also
keeps future 3D component APIs from coupling to the core effect runtime while
the catalog is still being evaluated. `HolographicSurface` and its typed
controls are explicitly preview API: their component shape, defaults, and
visual output may change before 1.0. Static SSR, reduced motion, runtime failure,
and explicit disablement retain semantic content and a CSS fallback.

## Package and CI isolation

CI packs both packages into `artifacts`. It runs `eng/verify-package.ps1` for
the core, `eng/verify-webgl-package.ps1` for the companion, and
`eng/verify-release-packages.ps1` to require exactly one package of each ID at
the same version. The main-branch release artifact includes both packages, and
the NuGet Trusted Publishing job pushes both through the same release gate.
Release tagging continues to derive unambiguously from the core package.

The dedicated verifier rejects Node artifacts and external executable imports
or fetches, requires the Three build files plus MIT license and provenance,
enforces a combined adapter/renderer budget below 1 MiB raw and 250 KiB Brotli,
and compiles a clean package consumer. The companion package also contains its
own NuGet README with install, registration, fallback, and preview guidance.

## Third-party and inspiration boundary

The companion vendors unmodified official Three.js r184 ESM build files under
MIT. Their source URLs and SHA-256 values are packaged with the license and
provenance record. ThreeUI informed the visual direction and candidate catalog,
but no ThreeUI source code, shaders, or assets are included. FancyBlazor's
adapter, material renderer, typed API, lifecycle, fallbacks, and demo are its
own implementation.

## Approved follow-up catalog

The published preview currently contains `HolographicSurface`. Phase 15 commits
the boundary's four approved follow-up components:

- `ParticleFieldBackground`
- `WaveFieldBackground`
- `RefractiveOrbBackground`
- `PrismFieldBackground`

Each component remains subject to the core accessibility, semantic DOM, static
fallback, reduced-motion, lifecycle cleanup, package, and documentation
contracts. The components are roadmap commitments but are not part of the
current published catalog until Phase 15 completes.

## Validation gate

The phase is complete only when local disposable-version commands produce both
packages in `artifacts`; the core, companion, and same-version release-set
verifiers pass; and the standard Release build/test/browser and documentation
gates pass. CI publishes the pair only from successful main-branch builds.

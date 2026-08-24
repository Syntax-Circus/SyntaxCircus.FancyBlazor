# Phase 13: WebGL Rendering Boundary

## Outcome

Phase 13 validates an **unpublished** `SyntaxCircus.FancyBlazor.WebGL` Razor
Class Library companion. ADR-013 selects this companion boundary instead of an
opt-in WebGL mode inside the published `SyntaxCircus.FancyBlazor` package.

The core package and its runtime remain unchanged. The companion owns its
registration (`AddFancyBlazorWebGl`), JavaScript lifecycle, CSS isolation, and
vendored Three.js r184 ESM assets. A clean Razor consumer restores the local
core and companion packages, registers the companion, and compiles without a
project reference, Node, npm, CDN, or a manual script import.

## Why a companion package

Keeping the renderer boundary separate prevents a Three.js payload and its GPU
lifecycle from becoming an opt-in-but-shipped cost of the core package. It also
keeps future 3D component APIs from coupling to the core effect runtime while
the catalog is still being evaluated. The phase proves package behavior only;
it does not make the companion a release, a NuGet publication candidate, or an
advertised consumer feature.

## Package and CI isolation

CI packs the published core package into `artifacts`. It packs the companion
only into `artifacts/webgl-spike` and runs `eng/verify-webgl-package.ps1` with
the root directory available solely as the companion's local transitive core
dependency source. The existing publication/upload wildcard is
`artifacts/*.*nupkg`, which matches root files only and cannot select the
nested spike package. No workflow uploads or pushes `artifacts/webgl-spike`.

The dedicated verifier rejects Node artifacts and external executable imports
or fetches, requires the Three build files plus MIT license and provenance,
enforces a combined adapter/renderer budget below 1 MiB raw and 250 KiB Brotli,
and compiles a clean package consumer.

## Approved future catalog candidates

The boundary is approved for evaluation of exactly these future components:

- `ParticleFieldBackground`
- `WaveFieldBackground`
- `RefractiveOrbBackground`
- `PrismFieldBackground`
- `HolographicSurface`

Any future component remains subject to the core accessibility, semantic DOM,
static fallback, reduced-motion, lifecycle cleanup, package, and documentation
contracts. This list is a scoped follow-up catalog, not a release commitment.

## Validation gate

The phase is complete only when the local disposable-version commands produce
the core package in `artifacts`, the companion package in
`artifacts/webgl-spike`, and both package verifiers pass. The standard Release
build/test/browser gates and documentation-link verification remain required.

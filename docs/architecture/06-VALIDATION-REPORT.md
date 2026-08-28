# FancyBlazor Validation Report

- **Date:** 2026-08-24
- **Candidate:** Unreleased (`0.2.1-preview.1` disposable local package version)
- **Result:** All implementation and release gates pass locally.

## Requirement evidence

| Requirement | Evidence |
| --- | --- |
| Four composable effects | RCL and Interactive Auto demo build all four components; bUnit verifies standalone and nested rendering contracts. |
| Semantic content and accessible decoration | Contract tests verify canvas/glare accessibility, stable hooks, merged attributes, and retained child markup; browser tests verify reveal and tilt interaction. |
| Typed setup/options | Unit tests verify registration defaults, overrides, palettes, quality, motion, pause behavior, and opt-in diagnostics. |
| Local, automatic assets | NuGet inspection verifies transitive CSS, JS, renderer, and Nacre assets; the clean consumer needs no script tag, Node, npm, or CDN. |
| Static SSR and interactive hosting | Static HTTP response, Interactive Auto demo, server/WASM registration, and a standalone WebAssembly host are built and exercised. |
| Reduced motion and failure fallback | Browser tests verify a final reduced-motion state and forced-WebGL-failure fallback without lost content/actions. |
| Hidden/offscreen efficiency | Browser diagnostics verify the Nacre RAF stops offscreen and resumes onscreen; quality caps bound DPR. |
| Complete disposal | Twenty enhanced-navigation mount/unmount cycles return the runtime registry to zero; destroy paths release observers, listeners, RAFs, and WebGL resources. |
| User and agent documentation | README, getting started, four API guides, five operational guides, compiling examples, conventions audit, and root `AGENTS.md` are present; local links are checked automatically. |
| Release packages | Core and optional WebGL `.nupkg`/`.snupkg` pairs are generated at the same version, required contents are inspected, and clean temporary Razor consumers restore and build from each package. |

## Final commands

```text
dotnet restore SyntaxCircus.FancyBlazor.slnx
dotnet build SyntaxCircus.FancyBlazor.slnx --no-restore --configuration Release
dotnet test --solution SyntaxCircus.FancyBlazor.slnx --no-build --configuration Release
pwsh eng/verify-docs.ps1
pwsh eng/tests/verify-webgl-package.tests.ps1
pwsh eng/tests/verify-core-package-selection.tests.ps1
pwsh eng/tests/verify-release-packages.tests.ps1
pwsh eng/tests/publish-nuget-packages.tests.ps1
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts/release-preview
dotnet pack src/SyntaxCircus.FancyBlazor.WebGL/SyntaxCircus.FancyBlazor.WebGL.csproj --no-build --configuration Release --output artifacts/release-preview
pwsh eng/verify-package.ps1 -PackageDirectory artifacts/release-preview
pwsh eng/verify-webgl-package.ps1 -PackageDirectory artifacts/release-preview -CorePackageDirectory artifacts/release-preview
pwsh eng/verify-release-packages.ps1 -PackageDirectory artifacts/release-preview
git diff --check
```

The current test result is 68 passed, zero failed, zero skipped across .NET,
bUnit, and Playwright. The Release build completes with zero warnings and zero
errors. Documentation verification checks 82 Markdown files.

The 0.1.5 test result is 33 passed, zero failed, zero skipped: sixteen
.NET/bUnit tests and seventeen Playwright tests. The browser run also emits
seven fixed-viewport, reduced-motion PNG artifacts under `TestResults/visual`.

## 0.1.5 extension evidence

The release adds nine CSS-first effects and five named composition presets.
Sixteen .NET/bUnit tests verify public rendering contracts and seventeen
Playwright tests cover the fallback matrix, static zero-runtime behavior,
interactive preset semantics, and seven reduced-motion visual artifacts.

## Audit scope

The accessibility gate validates library-owned semantics, focus neutrality,
reduced motion, and usable failure states; it is not a WCAG certification of a
consumer's colors, content, layout, or application. The performance gate
validates resource ceilings, offscreen/hidden behavior, high-frequency work
remaining in JavaScript, and deterministic cleanup; it does not claim a
cross-device frame-rate benchmark.

The local sandbox rejects GitVersion's repository ownership check. Local pack
validation therefore uses `DisableGitVersionTask=true` with the candidate
version explicitly supplied. The committed CI workflow uses an owned checkout,
full Git history, and GitVersion without that override.

## Phase 13 WebGL preview publication evidence

Phase 13 validates the optional `SyntaxCircus.FancyBlazor.WebGL` preview
companion with a disposable local version. CI packs the core and companion into
the same release artifact, and `eng/verify-release-packages.ps1` requires
exactly one package of each ID with matching versions before upload.
`eng/verify-webgl-package.ps1` inspects the companion for local Three.js r184
assets, its MIT license and SHA-256 provenance, Node/external-load exclusions,
its package README, and the adapter/renderer raw and Brotli budgets. It then
restores and builds a clean Razor consumer that references only the staged
packages and calls `AddFancyBlazorWebGl()`.

The main-branch NuGet Trusted Publishing job pushes both package files through
one authenticated release step. Tag derivation selects the core package
explicitly. ADR-013 records the separate renderer boundary, and ADR-014 records
same-version preview publication. Core-only consumers still request no WebGL
companion assets; companion consumers need no Node, npm, CDN, manual script
import, or project reference.

## Phase 16 UI companion publication evidence

Phase 16 adds `SyntaxCircus.FancyBlazor.UI` (`FancyButton`, `FancyLink`,
`FancyBadge`, `FancyCard`, `FancyNavbar`) as a third same-version companion.
All five controls are pure Razor plus scoped CSS and native HTML semantics:
the package ships no JavaScript, so `AddFancyBlazorUi()` registers only typed
theme options and chains `AddFancyBlazor()`.

Locally, all three packages were packed to a clean `artifacts/release-preview`
directory at a shared disposable version (`0.3.0-preview.1`) and verified:

```text
Verified SyntaxCircus.FancyBlazor.0.3.0-preview.1.nupkg: required assets present and clean Razor consumer builds.
Verified SyntaxCircus.FancyBlazor.WebGL.0.3.0-preview.1.nupkg: local Three assets, provenance, size budget, and clean Razor consumer (raw 45300 bytes; Brotli 13597 bytes).
Verified SyntaxCircus.FancyBlazor.UI.0.3.0-preview.1.nupkg: required assets present, no Bootstrap asset packed, core-only dependency graph, and clean Razor consumer builds.
Verified release package set: core, WebGL preview, and UI companion are version 0.3.0-preview.1.
```

`eng/verify-ui-package.ps1` rejects Node artifacts, rejects any packed
Bootstrap asset, requires the compiled assembly/README/notices/transitive
props, and restores+builds a clean Razor consumer that references only
`SyntaxCircus.FancyBlazor.UI` — inspecting its `project.assets.json` to
confirm the resolved dependency graph contains core but never
`SyntaxCircus.FancyBlazor.WebGL`. `eng/verify-release-packages.ps1` requires
exactly one package per ID with all three versions matching.

A dedicated Playwright suite (`UiCompanion_CoexistsCleanlyWithBootstrap5Reboot`)
renders all five controls with and without a locally vendored, unmodified
Bootstrap 5.3.3 stylesheet (`third-party/bootstrap/PROVENANCE.md`, test/demo
only) and asserts identical computed background color, text color, text
decoration, and border radius in both cases, proving ADR-016's coexistence
contract.

The full solution test run (.NET/bUnit plus Playwright) passed with all
existing core and WebGL coverage unaffected by the new package.

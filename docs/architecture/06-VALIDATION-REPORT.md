# FancyBlazor Validation Report

- **Date:** 2026-08-23
- **Candidate:** `0.1.5`
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
| Release package | `.nupkg` and `.snupkg` are generated, required contents are inspected, and a clean temporary Razor consumer restores and builds from the package. |

## Final commands

```text
dotnet restore SyntaxCircus.FancyBlazor.slnx
dotnet build SyntaxCircus.FancyBlazor.slnx --no-restore --configuration Release
dotnet test --solution SyntaxCircus.FancyBlazor.slnx --no-build --configuration Release
pwsh eng/verify-docs.ps1
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts
pwsh eng/verify-package.ps1 -PackageDirectory artifacts
git diff --check
```

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

## Phase 13 WebGL boundary spike evidence

Phase 13 validates an unpublished `SyntaxCircus.FancyBlazor.WebGL` companion
package with a disposable local version. The published core package remains in
`artifacts`; the companion package is packed only to `artifacts/webgl-spike`.
`eng/verify-webgl-package.ps1` inspects the companion for local Three.js r184
assets, its MIT license and SHA-256 provenance, Node/external-load exclusions,
and the adapter/renderer raw and Brotli budgets. It then restores and builds a
clean Razor consumer that references only the local packages and calls
`AddFancyBlazorWebGl()`.

The CI upload and publish paths remain `artifacts/*.*nupkg`; because this glob
does not recurse into `artifacts/webgl-spike`, the spike cannot be uploaded or
published by the existing core package workflow. ADR-013 records the selected
companion boundary and the five constrained future catalog candidates. This is
validation evidence, not a claim that the companion has shipped.

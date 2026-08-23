# FancyBlazor Validation Report

- **Date:** 2026-08-22
- **Candidate:** `0.1.0-preview.1`
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

The test result is 20 passed, zero failed, zero skipped: nine .NET/bUnit tests
and eleven Playwright tests. The browser run also emits four fixed-viewport,
reduced-motion PNG artifacts under `TestResults/visual`.

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

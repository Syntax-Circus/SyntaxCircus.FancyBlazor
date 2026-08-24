# WebGL Companion Boundary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an unpublished, locally packable `SyntaxCircus.FancyBlazor.WebGL` companion RCL with a production-shaped `HolographicSurface` proof and complete boundary evidence.

**Architecture:** Core remains unchanged at runtime. The companion owns a separate C#/JavaScript lifecycle, lazily loads vendored Three.js r184 only for visible eligible effects, and bounds GPU contexts with a FIFO pool.

**Tech Stack:** .NET 10 Razor Class Library, Blazor JS interop, Razor CSS isolation, Three.js r184 ESM, xUnit v3, Shouldly, bUnit, Playwright, PowerShell package verification.

**Spec:** Approved conversation plan for Phase 13 on 2026-08-23.

## Global Constraints

- Keep `ShaderBackground` and the core runtime behavior unchanged.
- Package ID is `SyntaxCircus.FancyBlazor.WebGL`; C# API uses `WebGl` casing and the root `SyntaxCircus.FancyBlazor` namespace.
- Three.js r184 stays internal and vendored with MIT license, SHA-256 provenance, no CDN, and no consumer Node/npm/tooling.
- `MaxActiveContexts` defaults to 4 and clamps to 1 through 8.
- Static SSR, reduced motion, failures, capacity waits, hidden/offscreen state, and disposal retain useful semantic content and a CSS fallback.
- Decorative canvases are `aria-hidden`, unfocusable, and pointer-transparent.
- Phase 13 validates a local package but does not publish or advertise the companion as shipped.

---

### Task 1: Companion API, component contracts, and registration

**Files:**
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/SyntaxCircus.FancyBlazor.WebGL.csproj`
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/FancyWebGlOptions.cs`
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/FancyBlazorWebGlServiceCollectionExtensions.cs`
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/Components/HolographicSurface.razor*`
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/Internal/*`
- Create: `tests/SyntaxCircus.FancyBlazor.WebGL.Tests/*`
- Modify: `SyntaxCircus.FancyBlazor.slnx`, test project references as required

**Interfaces:**
- Produce `AddFancyBlazorWebGl(Action<FancyWebGlOptions>? configure = null)` returning `IServiceCollection`.
- Produce `FancyWebGlOptions.MaxActiveContexts` with default 4 and options validation/clamping to 1–8.
- Produce `HolographicSurface` parameters `Palette`, `Intensity`, `Depth`, `Sheen`, `Speed`, `Interactive`, `Quality`, `Disabled`, `ChildContent`, `CssClass`, `Style`, and unmatched attributes.
- Produce internal create/update/destroy runtime calls; the module path is `./_content/SyntaxCircus.FancyBlazor.WebGL/js/fancy-blazor-webgl.js`.

- [x] Write bUnit and registration tests first for defaults, clamping, stable hooks, merged attributes, child semantics, disabled lifecycle, and option configuration.
- [x] Run the focused test project and record expected RED failures caused by missing companion types/project.
- [x] Implement the minimal RCL, C# runtime, component markup/code, and CSS fallback.
- [x] Run focused tests GREEN, then build the solution with the documented GitVersion sandbox override.
- [x] Commit the task and write the SDD report with RED/GREEN evidence.

### Task 2: Lazy Three.js renderer, context pool, and browser lifecycle proof

**Files:**
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/js/fancy-blazor-webgl.js`
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/js/holographic-surface-renderer.js`
- Create: `src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/*`
- Modify: test host, standalone host, browser test project, and browser tests

**Interfaces:**
- JS exports `createEffect`, `updateEffect`, `destroyEffect`, `disposeRuntime`, and diagnostic state.
- A lightweight bootstrap observes visibility and dynamically imports the renderer only after a FIFO pool slot is acquired.
- Each active instance owns one Three.js renderer/context; it fully releases on offscreen, hidden, disabled, context loss, or disposal and requeues when eligible.
- Diagnostic data reports instances, active/waiting contexts, frames, and whether Three.js loaded.

- [x] Write browser tests first for SSR fallback, lazy network loading, active rendering, fine-pointer response, reduced motion, forced failure, context loss, offscreen/hidden release, FIFO cap with five visible surfaces, semantic child activation/focus, parameter update, Auto/WASM hosting, and twenty navigation disposal cycles.
- [x] Run focused browser tests and record RED failures caused by missing runtime/assets/routes.
- [x] Vendor exact r184 ESM assets/license and record source URLs and SHA-256 values; implement the bootstrap, renderer, pool, lifecycle, and test-host pages.
- [x] Run browser tests GREEN and confirm no core-only page requests companion assets.
- [x] Commit the task and write the SDD report with RED/GREEN evidence.

### Task 3: Package isolation, CI validation, and architecture records

**Files:**
- Create: `eng/verify-webgl-package.ps1`
- Modify: `.github/workflows/build.yml`, notices/provenance/license files, `AGENTS.md`, `CHANGELOG.md`, and architecture documents
- Create: `docs/architecture/PHASE-13-webgl-rendering-boundary.md`

**Interfaces:**
- CI packs the companion under `artifacts/webgl-spike/`, verifies it, and never includes it in `artifacts/*.*nupkg` publication inputs.
- Verification rejects Node artifacts and external executable imports/fetches, builds a clean package consumer, and enforces combined adapter/renderer JS below 1 MiB raw and 250 KiB Brotli.
- ADR-013 selects the companion package and records the five approved future components.

- [x] Add package-verifier failure cases or script-level assertions before changing pack/CI behavior; record RED evidence.
- [x] Implement nested packing, package inspection, clean-consumer validation, licensing/provenance, documentation, roadmap/index/validation updates, and the follow-up catalog phase.
- [x] Run documentation and both package verification scripts GREEN.
- [x] Run Release restore/build/tests/pack, browser tests, and `git diff --check` with required sandbox overrides.
- [x] Commit the task and write the SDD report with verification evidence.

# FancyBlazor Implementation Roadmap

| Order | Phase | Status | Depends on | Exit evidence |
| --- | --- | --- | --- | --- |
| 1 | Foundation | Complete | Approved plan | Restorable/buildable project graph and architecture contracts |
| 2 | Proof of Fancy | Complete | Foundation | Nacre lifecycle and cross-host fallback/disposal tests |
| 3 | Effects and Demo | Complete | Proof of Fancy | Four documented, tested, composable components |
| 4 | Hardening and Preview | Complete | All prior phases | Passing test/package/release audit |
| 5 | Expressive Catalog Expansion | Complete | Hardening and Preview | Six documented, tested text, ambient, and pointer effects |
| 6 | Spatial Surfaces | Complete | Expressive Catalog Expansion | Five documented, tested CSS-first surface effects |
| 7 | Narrative Motion | Complete | Spatial Surfaces | Three documented, tested in-flow semantic storytelling effects |
| 8 | Interaction Feedback | Complete | Narrative Motion | Three documented, tested decorative hover, press, and focus wrappers |
| 9 | Quality, Accessibility, and Compatibility | Complete | Interaction Feedback | Browser fallback matrix, performance budgets, visual regression, and public-API review |
| 10 | Composition and Authoring | Complete | Quality, Accessibility, and Compatibility | Compile-tested composition recipes and named visual presets |
| 11 | CSS-First Catalog Expansion | Complete | Quality, Accessibility, and Compatibility | Selected typography, separator, and surface treatments |
| 12 | v0.2.0 Atmosphere and Accents | Complete | CSS-First Catalog Expansion | Six documented, tested Canvas 2D, typography, and child-preserving accent components |
| 13 | WebGL Rendering Boundary and Preview | Complete (preview) | v0.2.0 Atmosphere and Accents | ADR-013 selects the companion boundary; ADR-014 publishes it at the exact core version with isolated pack/consumer proof and vendored Three r184 provenance |
| 14 | [Core Kinetic Catalog](PHASE-14-core-kinetic-catalog.md) | Complete | WebGL Rendering Boundary and Preview | Six semantic, fallback-safe kinetic and atmospheric effects |
| 15 | [WebGL Fields and Materials](PHASE-15-webgl-fields-and-materials.md) | Complete | Core Kinetic Catalog | Four typed Three.js effects with lifecycle, fallback, and package-budget proof |
| 16 | [UI Companion Foundation](PHASE-16-ui-companion-foundation.md) | Approved | WebGL Fields and Materials | Exact-version core-only companion package with tokens and five accessible primitives |
| 17 | [Marketing and Content UI](PHASE-17-marketing-and-content-ui.md) | Approved | UI Companion Foundation | Seven slot-driven site controls with accessible interaction and compiling examples |
| 18 | [1.0 Stabilization](PHASE-18-1.0-stabilization.md) | Approved | All prior phases | Resolved preview APIs, migration notes, and cross-package release-readiness evidence |

A phase is complete only when its task boxes and validation gate are supported by current command output or inspected artifacts.

See [06-VALIDATION-REPORT.md](06-VALIDATION-REPORT.md) for the current evidence.

## Evaluation bank after the committed phases

These are candidates for later selection, not release commitments or frozen
public APIs.

- **Core:** `WordRotate`, `MorphText`, `ScrollVelocity`,
  `CausticsBackground`, `TopographicBackground`, `RainBackground`,
  `CompareReveal`, `Lens`, and additional composition presets.
- **WebGL:** `LiquidMetalSurface`, `IridescentBlobBackground`,
  `NebulaFieldBackground`, `PortalBackground`, `DepthGridBackground`, and
  `CrystalFieldBackground`.
- **UI:** tabs, carousel, timeline, statistics and media shells, breadcrumbs,
  pagination, dialogs, drawers, popovers, tooltips, toasts, form controls,
  tables, empty states, and loading states.
- **Future integration:** evaluate a separately installed WebGL-enhanced UI
  layer only after the core-only UI companion proves its contracts.

Candidate selection weighs reuse, accessibility, fallback quality,
payload/lifecycle cost, and distinctness from the existing catalog.

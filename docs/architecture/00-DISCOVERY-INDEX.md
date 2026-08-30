# FancyBlazor Discovery Index

- **Status:** Seventeen implementation phases complete; Phase 18 (1.0 Stabilization) remains the only approved pre-1.0 work
- **Project brief:** [../PROJECT-PLAN.md](../PROJECT-PLAN.md)
- **Requirements:** [01-REQUIREMENTS.md](01-REQUIREMENTS.md)
- **Architecture:** [02-ARCHITECTURE.md](02-ARCHITECTURE.md)
- **Package map:** [03-PACKAGE-MAP.md](03-PACKAGE-MAP.md)
- **Decisions:** [04-DECISION-LOG.md](04-DECISION-LOG.md)
- **Syntax Circus conventions audit:** [05-CONVENTIONS-AUDIT.md](05-CONVENTIONS-AUDIT.md)
- **Validation report:** [06-VALIDATION-REPORT.md](06-VALIDATION-REPORT.md)
- **Demo UX brief:** [UX-BRIEF-demo.md](UX-BRIEF-demo.md)
- **Roadmap:** [99-IMPLEMENTATION-ROADMAP.md](99-IMPLEMENTATION-ROADMAP.md)

## Phase order

1. [Foundation](PHASE-01-foundation.md)
2. [Proof of Fancy](PHASE-02-proof-of-fancy.md)
3. [Representative Effects](PHASE-03-effects-and-demo.md)
4. [Hardening and Preview](PHASE-04-hardening-and-preview.md)
5. Expressive Catalog Expansion
6. [Spatial Surfaces](PHASE-06-spatial-surfaces.md)
7. [Narrative Motion](PHASE-07-narrative-motion.md)
8. [Interaction Feedback](PHASE-08-interaction-feedback.md)
9. [Quality, Accessibility, and Compatibility](PHASE-09-quality-accessibility-compatibility.md)
10. [Composition and Authoring](PHASE-10-composition-and-authoring.md)
11. [CSS-First Catalog Expansion](PHASE-11-css-first-catalog-expansion.md)
12. v0.2.0 Atmosphere and Accents
13. [WebGL Rendering Boundary](PHASE-13-webgl-rendering-boundary.md)
14. [Core Kinetic Catalog](PHASE-14-core-kinetic-catalog.md)
15. [WebGL Fields and Materials](PHASE-15-webgl-fields-and-materials.md)
16. [UI Companion Foundation](PHASE-16-ui-companion-foundation.md)
17. [Marketing and Content UI](PHASE-17-marketing-and-content-ui.md)
18. [1.0 Stabilization](PHASE-18-1.0-stabilization.md)

## Kinetic text batch (2026-08-30)

A post-Phase-17 batch added `WordRotate`, `MorphText`, and `Typewriter` to the
core catalog. All three share the existing `IFancyEffectRuntime` +
`fancy-blazor.js` dispatcher and follow the `ScrambleText` and `NumberTicker`
component pattern. The visible motion is `aria-hidden`; the host exposes a
complete accessible text mirror. Each component settles to the first word or
line and adds the `syntax-circus-fancy-kinetic-text--static` class on the host
when `Disabled` is `true` or when the system requests reduced motion. See
[the design spec](../superpowers/specs/2026-08-30-core-effects-kinetic-text-batch-design.md)
and [the implementation plan](../superpowers/plans/2026-08-30-core-kinetic-text-batch.md).

## Approval record

The project owner selected a focused four-component preview, vendored upstream
assets identified by checksums, required `AddFancyBlazor()` registration,
`.NET 10` hosting-mode support, internal-only extension seams, automatic shader
fallback, `TimeSpan` timing, a small deterministic visual baseline, a focused
demo, and the full Syntax Circus architecture artifact suite.

## Completion checklist

- [x] Goals, users, scope, and non-goals are explicit.
- [x] Architecture and package boundaries are explicit.
- [x] Material decisions are accepted.
- [x] Phase dependencies and validation gates are defined.
- [x] Demo UX has an implementation handoff.
- [x] All implemented phases pass their completion gates.

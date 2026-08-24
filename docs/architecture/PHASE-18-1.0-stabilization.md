# PHASE-18: 1.0 Stabilization

## Objective

Resolve the preview APIs and prove release readiness across the core, WebGL, and
UI packages before the first stable release.

## Actionable tasks

- [ ] Review every public component, parameter, enum, default, registration
  method, rendered hook, CSS custom property, and documented behavior across all
  three packages; resolve or document every intentional breaking change.
- [ ] Publish migration notes from the latest preview and finalize the stable
  compatibility, deprecation, and versioning policy.
- [ ] Complete accessibility, static SSR, reduced-motion, failure-fallback,
  hosting-mode, browser-compatibility, performance, lifecycle, and visual
  baseline matrices for every component family.
- [ ] Lock package payload budgets, third-party provenance, exact-version
  dependencies, three-package release verification, and clean consumers with no
  Node, npm, CDN, manual scripts, or project references.
- [ ] Reconcile README, user guides, demos, contributor contracts, changelog,
  package READMEs, release automation, and validation evidence with the final API.

## Success criteria

All preview decisions are resolved, migration guidance is complete, and the
three-package release set passes every documented build, test, package,
accessibility, lifecycle, and clean-consumer gate.

## Validation gate

Run the complete local command matrix from `AGENTS.md` plus all three package
content verifiers and the exact-version release-set verifier, then record current
evidence in `06-VALIDATION-REPORT.md` before releasing 1.0.

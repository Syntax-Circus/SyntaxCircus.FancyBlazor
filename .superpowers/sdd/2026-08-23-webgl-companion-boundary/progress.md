# SDD ledger — plan: docs/superpowers/plans/2026-08-23-webgl-companion-boundary.md

## Baseline

- Branch: `feature/webgl`; normal checkout explicitly approved by user.
- Worktree clean before setup.
- Unit baseline: 18/18 passing with `DisableGitVersionTask=true` and disposable package version.
- Browser baseline: Playwright browser install was missing under sandbox; install returned success but browser-path access remains sandbox-restricted. Final browser runs must use the approved escalated `dotnet test` path.
- GitVersion sandbox limitation is documented by the repository; all local validation uses `-p:DisableGitVersionTask=true -p:PackageVersion=0.2.1-preview.1`.

## Pre-flight dependency scan

| Producer | Consumer | Shared interface/file | Finding |
| --- | --- | --- | --- |
| Task 1 | Task 2 | Companion project, `HolographicSurface`, internal C# runtime contract | Clean: Task 2 consumes the exact create/update/destroy and module path produced by Task 1. |
| Task 1 | Task 3 | Companion project/package metadata and solution entry | Clean: Task 3 packages the project created by Task 1. |
| Task 2 | Task 3 | Static assets, diagnostics, browser evidence, provenance | Clean: Task 3 verifies and documents Task 2 outputs. |
| Task 1 | Task 1 | Tests precede public API/component implementation | Clean. |
| Task 2 | Task 2 | Browser tests precede runtime/assets implementation | Clean; vendored generated assets are exempt from TDD but lifecycle behavior is not. |
| Task 3 | Task 3 | Verification assertions precede CI/docs completion | Clean. |

## Rulings

- Ruling: the companion uses a separate internal runtime rather than exposing or friending core internals — preserves core API/runtime isolation — costs a small amount of intentionally duplicated lifecycle plumbing.
- Ruling: official Three.js r184 ESM artifacts are vendored unchanged — removes maintainer bundling from the spike and makes provenance reproducible — costs a larger companion payload, bounded by the approved size gate.
- Ruling: the source seed is locally packable but CI output goes only to `artifacts/webgl-spike/` — proves NuGet behavior without reaching the existing publication wildcard.

## Task status

- Task 1: complete — commits `91e4c61`, `c394f62`; focused tests 10/10 pass; task re-review approved with no remaining findings.
- Task 2: fix round 1 complete — `three.core.js` is now vendored with the exact r184 transitive-module regression; focused Release browser build and module-graph/pointer-update tests pass. Commit pending.
- Task 2: review fix round 2 implemented — construction ownership guard, strict fine-pointer gating, palette-uniform updates, enhanced lifecycle proofs, and unlinked Interactive Auto probe added. Fresh focused Release build passed; named coarse-pointer and Auto proofs pass. The delayed-construction reporter result needs a clean-controller rerun because the tool captured no completion line.
- Task 2: review fix round 3 implemented — stale renderer destruction is counted through one internal helper and the delayed-construction regression disposes the active page while construction is in flight, proving that live renderer objects converge to zero with the runtime. Focused Release build passed. The named browser run was externally aborted after 285 seconds without a completion result; controller rerun required.
- Task 2: review fix round 3 verified — the race proof now deterministically disposes during delayed construction through enhanced navigation, asserting zero instances, contexts, and live renderer objects plus balanced create/destroy counts. Controller's named test passed 1/1 in 4.385 seconds with the 2000 ms hook; Release browser-test build passed with 0 warnings/errors.
- Task 2: review fix round 3 suite-stability correction implemented — replaced millisecond race timing with a same-page construction gate, replaced transformed offscreen proof with deterministic layout removal, and corrected fallback semantics assertion. Focused Release browser-test build passed with 0 warnings/errors; controller will rerun the wildcard suite.
- Task 2: review fix round 4 verified — lifecycle diagnostics expose actual FIFO handles and existing test IDs; offscreen waiters leave the queue; renderer release now carries an idempotent lost-context restore handshake across reactivation with post-await ownership checks. The named FIFO/context-loss/hidden-document browser proof passed 1/1 in 2.268 seconds and the focused Release build passed with 0 warnings/errors; the wildcard suite was not run per controller instruction.
- Task 2: review fix round 4 suite cleanup verified — the in-flight construction proof now observes any slot-owning loading surface instead of hard-coding a DOM-order winner. The named proof passed 1/1 in 1.947 seconds and the focused Release build passed with 0 warnings/errors; no runtime files changed.
- Task 2: minor (deferred): diagnostics set `threeLoaded` only after renderer creation, so a successful Three import followed by WebGL construction failure reports false.
- Task 2: complete (commits `c394f62..85c98f5`, review clean; complete focused HolographicSurface suite passed 8/8 together).
- Task 3: implementation complete pending controller package-verifier rerun. RED rejection harness observed the verifier missing; GREEN harness rejects Node artifacts and HTTPS executable imports. Release solution build passed 0 warnings/errors; core and nested companion preview packages were created. Docs verifier passed. Clean-consumer restore is blocked by sandbox `NU1301` network access; escalated rerun was interrupted after 360.9 seconds. CI keeps companion output only in `artifacts/webgl-spike` outside the root upload/publication glob. Commit recorded in the task handoff.
- Task 3: review fix round 1 complete — direct typed per-entry byte accounting replaces pipeline-flattened arrays; package exclusions include `package.json` and common Node metadata; owned adapter/renderer URL detection conservatively rejects HTTP(S) and protocol-relative literals. RED harness added raw-budget, Node manifest, static/dynamic import, fetch, importScripts, and protocol-relative cases; focused harness GREEN. Three notices/license/provenance remain owned by Task 2 (`46c18f8`) and are packaged without duplicate edits. Commit pending.
- Task 3: scoped re-review round 1 complete — restored package-wide `.js` inspection including vendor paths; external detection now rejects quoted HTTP(S)/protocol-relative literals without treating `//todo` comments as URLs. Vendor-fetch regression was RED against the narrowed scan and GREEN after the fix; ordinary-comment precondition reaches only the expected missing-core clean-consumer step. Commit pending.
- Task 3: commits `0683520`, `b008cea`; report/ledger handoff committed separately because `.superpowers` is ignored by default.
- Final review fix wave: complete pending commit — renderer now retains finite zero values (including create/update `Speed=0`), caches drawing-buffer dimensions/DPR, uses geometry UVs, reconciles `Interactive` listeners, responds to live reduced-motion changes, and removes the media listener during runtime disposal. The fallback gradient is valid and browser-tested. Async failures retain fallback and warn once; `threeLoaded` is set immediately after Three resolves before construction. RED: new browser tests failed before implementation because renderer diagnostics/behavior were absent (zero/resize state unavailable; one-shot reduced-motion handling could not satisfy the transition). GREEN: focused Release build passed with 0 warnings/errors; new zero-speed, interaction/resize/UV, and live-reduced-motion/async-failure browser tests each passed 1/1. Existing eight HolographicSurface proofs passed in the focused wildcard run before test-wait refinements; no production change followed those refinements.

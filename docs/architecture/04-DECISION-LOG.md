# FancyBlazor Decision Log

All decisions below are accepted.

| ID | Decision | Consequence |
| --- | --- | --- |
| ADR-001 | Ship one `SyntaxCircus.FancyBlazor` RCL. | No provider/package fragmentation in preview. |
| ADR-002 | Blazor owns public APIs; engines stay internal. | No raw uniforms, slugs, handles, or public providers. |
| ADR-003 | JavaScript owns high-frequency work. | Blazor sends create/update/destroy only. |
| ADR-004 | Preserve semantic DOM and progressively enhance it. | Static SSR and failure states remain useful. |
| ADR-005 | Vendor shader.gallery `0.1.0` by checksum. | No consumer CDN/Node; updates require provenance review. |
| ADR-006 | Require `AddFancyBlazor()` and root namespace APIs. | Shared runtime/options with one import. |
| ADR-007 | Target `net10.0` across Blazor modes. | Hosts cover Server, WASM, Auto, and prerendering. |
| ADR-008 | Preview four representative effects. | Future catalog and extension APIs remain backlog. |
| ADR-009 | Use `TimeSpan` and automatic fallback. | Idiomatic timing and no fallback slot requirement. |
| ADR-010 | Docs, examples, demo, and `AGENTS.md` are release artifacts. | Public changes update all contracts together. |
| ADR-011 | v0.2.0 adds clean-room Canvas 2D, text, and decorative accent effects without a new rendering engine. | No ThreeUI source/assets, Three.js runtime, full-page scenes, or UI controls enter the main RCL. |
| ADR-012 | Defer richer WebGL/3D work to a post-v0.2.0 package-versus-mode spike. | Evaluate `FancyBlazor.WebGL` against an opt-in main-package mode before committing a renderer boundary. |

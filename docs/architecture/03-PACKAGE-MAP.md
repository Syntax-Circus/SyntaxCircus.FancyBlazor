# FancyBlazor Package and Dependency Map

## Syntax Circus packages

| Concern | Status | Package | Purpose and boundary | Owning phase |
| --- | --- | --- | --- | --- |
| Reusable host utility UI | Excluded | `SyntaxCircus.Blazor.Components` | Separate error/not-found/reconnect concern; the planned UI companion does not absorb shared host utilities. | — |
| Optional 3D rendering | Published preview | `SyntaxCircus.FancyBlazor.WebGL` | Separately installed companion RCL selected by ADR-013; owns its Three.js r184 assets and runtime without changing the core package payload. It publishes at the exact core package version. | 13 |
| Styled site controls | Published preview | `SyntaxCircus.FancyBlazor.UI` | Optional exact-version companion selected by ADR-015; owns accessible marketing/content widget semantics and scoped themes, depends only on core, and does not replace shared host-utility components. | 16–17 |
| Other catalog concerns | Not applicable | — | No auth, persistence, HTTP, storage, email, analytics, or business integration exists here. | — |

## NuGet dependencies

| Package | Version | Boundary |
| --- | --- | --- |
| `GitVersion.MsBuild` | 6.6.0 | Private build-time versioning |
| `bunit` | 2.9.0 | Test only |
| `Microsoft.NET.Test.Sdk` | 18.9.0 | Test only |
| `xunit.v3` | 3.2.2 | Test only |
| `xunit.runner.visualstudio` | 3.1.5 | Test only |
| `Shouldly` | 4.3.0 | Test only |
| `Microsoft.Playwright` | 1.62.0 | Browser test only |
| ASP.NET Core WebAssembly packages | 10.0.11 | Demo hosting only |

shader.gallery CLI, runtime, and Nacre are vendored snapshot `0.1.0`, MIT.
Integrity and SHA-256 values are recorded in packaged provenance; no npm
dependency reaches consumers.

The WebGL preview companion depends on the same-version core package and vendors
Three.js r184 ESM assets with the MIT text and SHA-256 provenance. Its
adapter/renderer has an isolated size gate. CI stages both packages together,
verifies their versions match, then publishes them through one release job.

Phase 16 extends that same-version release set to the planned UI package. The UI
companion must reference core at the exact version and must not reference the
WebGL companion. Its package-content and clean-consumer gates become release
requirements before its first publication.

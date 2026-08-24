# FancyBlazor Package and Dependency Map

## Syntax Circus packages

| Concern | Status | Package | Purpose and boundary | Owning phase |
| --- | --- | --- | --- | --- |
| Reusable utility UI | Excluded | `SyntaxCircus.Blazor.Components` | Separate error/not-found/reconnect concern; FancyBlazor owns visual effects only. | — |
| Optional 3D rendering | Validated, unpublished | `SyntaxCircus.FancyBlazor.WebGL` | Separate Phase 13 companion RCL selected by ADR-013; owns its Three.js r184 assets and runtime without changing the published core package. | 13 |
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

The unpublished WebGL companion vendors Three.js r184 ESM assets with the MIT
text and SHA-256 provenance in its package. Its adapter/renderer has an isolated
size gate, and its local artifacts are nested below `artifacts/webgl-spike` so
the root publication glob cannot select them.

# FancyBlazor Package and Dependency Map

## Syntax Circus packages

| Concern | Status | Package | Purpose and boundary | Owning phase |
| --- | --- | --- | --- | --- |
| Reusable utility UI | Excluded | `SyntaxCircus.Blazor.Components` | Separate error/not-found/reconnect concern; FancyBlazor owns visual effects only. | — |
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

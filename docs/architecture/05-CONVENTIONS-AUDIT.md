# Syntax Circus Conventions Audit

## References inspected

- sibling `_template`: discovery workflow, agent guide, architecture artifacts,
  phase template, package-map policy, and repository structure;
- sibling `SyntaxCircus.Blazor.Components`: package project, build props, central
  versions, solution, workflow, README, tests, and `AGENTS.md`.

## Rules inherited by FancyBlazor

| Convention | FancyBlazor application |
| --- | --- |
| Discovery precedes implementation | Requirements, architecture, package map, decisions, UX brief, phases, and roadmap are maintained under `docs/architecture`. |
| Make package boundaries explicit | The focused core RCL owns general effects; the optional preview companion owns Three.js-backed effects and GPU lifecycle. Hosts retain layout, routing, content, design system, and application policy. |
| Prefer reusable Blazor contracts | Public APIs are typed Razor components in one root namespace and make no assumptions about a host CSS framework. |
| Use the current .NET baseline | All projects target `net10.0`, pinned by `global.json`. |
| Centralize dependencies | All dependency versions are in `Directory.Packages.props`; project references have no inline versions. |
| Enforce build quality | Nullable references, recommended analyzers, and warnings-as-errors apply repository-wide. |
| Derive package versions from Git | `GitVersion.MsBuild` is private and CI checks out full history. |
| Publish a complete NuGet | Package metadata includes README, MIT license expression, symbols, repository metadata, and transitive static web assets. |
| Keep consumer docs contractual | README, parameter guides, compiling examples, tests, and `AGENTS.md` change together with public behavior. |
| Use the established test stack | xUnit v3, Shouldly, and bUnit cover .NET/rendering contracts. |
| Automate release flow | CI restores, builds, tests, packs, validates, uploads, uses NuGet Trusted Publishing, and tags the published version. |

## Intentional FancyBlazor differences

`SyntaxCircus.Blazor.Components` is mostly presentation-only and therefore
avoids service registration, library styling, and general JavaScript. Those are
not universal bans: FancyBlazor's approved concern is visual effects, so it
ships isolated effect CSS, a packaged ES module, and required scoped runtime
registration. It still avoids global resets, consumer script imports, Node,
CDNs, framework islands, host layouts, and application policy.

FancyBlazor also adds Playwright projects, a multi-mode demo, a standalone
WebAssembly consumer, deterministic visual artifacts, third-party provenance,
and clean-package-consumer verification. These are required by its browser and
WebGL risk profile and are stronger gates than a markup-only component needs.

Both RCLs reference `Microsoft.AspNetCore.Components.Web` instead of the shared
ASP.NET Core framework so they remain consumable by standalone WebAssembly.
Publishing a second package is an intentional payload and lifecycle boundary:
core-only consumers never receive Three.js, while companion consumers opt into
the preview renderer at the exact core version.

## Ongoing rule

When a new `SyntaxCircus.*` package is proposed, begin with the template's
discovery artifacts, copy repository mechanics from the nearest maintained
sibling, and preserve only the rules that match the package's actual concern.
Any intentional difference must be recorded in the decision log and backed by
tests proportional to its runtime risk.

# Project Brief: FancyBlazor

FancyBlazor is an MIT-licensed visual-effects component library that gives
Blazor applications polished backgrounds, borders, reveals, and pointer motion
without React islands, runtime CDNs, or consumer JavaScript build tooling. Its
approved pre-1.0 roadmap keeps those effects in core and WebGL packages while
adding an optional styled UI companion for reusable site controls.

## Preview outcome

The repository publishes a `net10.0` core package,
`SyntaxCircus.FancyBlazor`, plus the separately installed
`SyntaxCircus.FancyBlazor.WebGL` preview companion at the exact same version.
The initial preview proved four representative rendering paths:

- `ShaderBackground` with the vendored Nacre WebGL shader;
- `GlowBorder`, implemented CSS-first;
- `Reveal`, driven by `IntersectionObserver`;
- `Tilt`, driven by pointer input.

A focused Interactive Auto demo, complete user guides, compiling examples,
agent instructions, and automated .NET/browser/package tests are release
requirements.

## Product rules

- Blazor components and C# types own the public API. Rendering engines remain
  implementation details.
- Meaningful child content remains semantic DOM. Canvas and glare layers are
  decorative.
- JavaScript owns high-frequency rendering and always disposes its resources.
- Static SSR, reduced motion, missing JavaScript, and missing WebGL retain usable
  content and a palette-derived fallback.
- Effect styles are scoped and CSS-framework-agnostic.
- The preview exposes internal extension seams only. Custom shaders, public
  providers, and additional engines remain out of scope. The approved roadmap
  expands the typed effect catalog and introduces the separately installed
  `SyntaxCircus.FancyBlazor.UI` companion without moving widget semantics into
  core or making WebGL a transitive UI dependency.

The completed catalog expansions add text, ambient-surface,
pointer-interaction, spatial-surface, narrative-motion, and interaction-feedback
effects. Optional Three.js-backed work remains isolated in the WebGL companion;
both packages preserve the same progressive-enhancement and semantic-DOM model.

## Delivery plan

The approved requirements, architecture, decisions, dependency inventory, phase
plans, and gates are indexed in
[architecture/00-DISCOVERY-INDEX.md](architecture/00-DISCOVERY-INDEX.md).

Consumer documentation starts at the repository [README](../README.md) and
[user documentation index](README.md).

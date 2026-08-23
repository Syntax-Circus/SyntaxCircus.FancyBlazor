# Project Brief: FancyBlazor

FancyBlazor is an MIT-licensed visual-effects component library that gives
Blazor applications polished backgrounds, borders, reveals, and pointer motion
without React islands, runtime CDNs, or consumer JavaScript build tooling.

## Preview outcome

`0.1.0-preview.1` contains one package, `SyntaxCircus.FancyBlazor`, targeting
`net10.0`. The preview proves four representative rendering paths:

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
providers, additional engines, and the larger effect catalog are backlog.

The next catalog expansion adds text, ambient-surface, and pointer-interaction
effects while retaining the same single-package, progressive-enhancement model.

## Delivery plan

The approved requirements, architecture, decisions, dependency inventory, phase
plans, and gates are indexed in
[architecture/00-DISCOVERY-INDEX.md](architecture/00-DISCOVERY-INDEX.md).

Consumer documentation starts at the repository [README](../README.md) and
[user documentation index](README.md).

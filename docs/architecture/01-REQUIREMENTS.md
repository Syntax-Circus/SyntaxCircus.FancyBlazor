# FancyBlazor Requirements

## Goal and users

Blazor developers need reusable visual polish without adopting a second UI
framework or learning WebGL. A developer should install the package, register
it, import one namespace, and render an obvious effect in under five minutes.
The approved pre-1.0 roadmap extends that experience with a separately installed
UI companion for visually expressive, accessible marketing and content controls.

## Functional requirements

- Provide `ShaderBackground`, `GlowBorder`, `Reveal`, and `Tilt` as composable Razor components.
- Preserve arbitrary child content as real DOM and provide stable classes, CSS variables, `CssClass`, `Style`, and attribute splatting.
- Provide typed options, palettes, quality, motion preferences, and global defaults through `AddFancyBlazor()`.
- Load packaged JavaScript and shader assets without consumer imports, Node, npm, CDN access, or a network request outside the host.
- Supply a focused demo and compiling example for every public component.

## Non-functional requirements

- Target `net10.0` and support static SSR plus Interactive Server, Interactive WebAssembly, Interactive Auto, and standalone WebAssembly.
- Respect `prefers-reduced-motion` by default.
- Stop continuous rendering when materially offscreen or the document is hidden.
- Release all observers, listeners, frames, and WebGL resources on disposal.
- Retain usable content and a static visual fallback when initialization fails.
- Remain independent of CSS frameworks and host layouts.
- Treat warnings as errors and publish symbols and repository metadata.

## Explicitly out of scope and package boundaries

- General-purpose controls or a design system in the core or WebGL packages. The
  planned UI companion is the only package that may own widget semantics.
- Consumer-provided shaders, custom GLSL, public provider registration, or a
  public JavaScript extension API.
- Three.js, raw shaders, and 3D renderer lifecycle in the core package. The
  optional WebGL preview companion may vendor a bounded Three.js runtime behind
  typed components and automatic fallbacks.
- Paper Shaders, WebGPU, arbitrary custom GLSL, or a full shader playground.
- Runtime CDN loading, consumer JavaScript build tooling, or framework islands.

## Approved pre-1.0 roadmap direction

- Expand core with typed kinetic text/content accents and bounded CSS/Canvas
  atmospheric fields while preserving semantic DOM and static fallbacks.
- Expand WebGL with the four approved typed fields and materials without adding
  another rendering engine or exposing Three.js internals.
- Add `SyntaxCircus.FancyBlazor.UI` as an optional exact-version companion. Its
  public types remain in `SyntaxCircus.FancyBlazor`, registration is provided by
  `AddFancyBlazorUi()`, and it depends only on core.
- Keep routing, authentication, business logic, global resets, host typography,
  and product-specific content outside every FancyBlazor package.

## Measurable acceptance

- No active FancyBlazor animation frame remains for a hidden/offscreen effect.
- Twenty mount/unmount cycles return the runtime registry to zero instances.
- Clean projects build independently from the currently packed core and WebGL
  NuGet packages without project references; both release package versions match.
- Once Phase 16 publishes UI, isolated core, WebGL, and UI consumers build from
  the three matching package versions, and the UI consumer graph contains core
  but not WebGL.
- Every documented snippet maps to compiling sample source.
- All Release builds, .NET tests, browser tests, and package inspections pass.

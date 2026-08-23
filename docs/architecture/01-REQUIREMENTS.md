# FancyBlazor Requirements

## Goal and users

Blazor developers need reusable visual polish without adopting a second UI
framework or learning WebGL. A developer should install the package, register
it, import one namespace, and render an obvious effect in under five minutes.

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

## Explicitly out of scope for preview

- General-purpose controls or a design system.
- More shaders, custom GLSL, public provider registration, or a public JavaScript extension API.
- Paper Shaders, Three.js, WebGPU, particles, text effects, or a full playground.
- Runtime CDN loading, consumer JavaScript build tooling, or framework islands.

## Measurable acceptance

- No active FancyBlazor animation frame remains for a hidden/offscreen effect.
- Twenty mount/unmount cycles return the runtime registry to zero instances.
- A clean project builds from the packed NuGet package without project references.
- Every documented snippet maps to compiling sample source.
- All Release builds, .NET tests, browser tests, and package inspections pass.

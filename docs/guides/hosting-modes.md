# Blazor Hosting Modes

## Static SSR and Interactive Server

Register `AddFancyBlazor()` in the server project. Static SSR emits child markup
and fallback styling; effects initialize only after an interactive render.

## Interactive WebAssembly and standalone WebAssembly

Register `AddFancyBlazor()` in WebAssembly `Program.cs`. The package targets the
browser platform and loads its module from `_content/SyntaxCircus.FancyBlazor/`.

## Interactive Auto

Register the package in both server and `.Client` projects. Place routed effects
in the client assembly so the same component can run through Server or
WebAssembly interactivity. The repository demo is the canonical configuration.

No hosting mode needs a manual JavaScript import or runtime CDN.

## Optional WebGL preview companion

Install `SyntaxCircus.FancyBlazor.WebGL` and call `AddFancyBlazorWebGl()` in
each executable host; the companion registration also adds core defaults. Call
`AddFancyBlazor(...)` first only when configuring shared options. Interactive
Auto requires the companion registration in both server and `.Client` projects.
Its module and vendored Three.js files load from
`_content/SyntaxCircus.FancyBlazor.WebGL/` without Node, npm, a CDN, or a manual
script import.

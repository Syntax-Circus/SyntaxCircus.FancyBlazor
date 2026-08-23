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

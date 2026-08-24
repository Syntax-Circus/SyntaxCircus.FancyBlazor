# FancyBlazor Architecture

## Topology

The published core Razor Class Library contains its C# API, Razor components,
isolated CSS, FancyBlazor's JavaScript adapter, and vendored shader-gallery
assets. A separately installed preview companion RCL owns optional Three.js
effects. Both packages share one GitVersion-derived release version, while the
companion renderer and lifecycle never join the core runtime or core package.
The demo and test projects are non-packable consumers of both boundaries.
The approved roadmap adds a third, separately installed UI RCL. It shares the
release version and public namespace, depends on core only, and owns accessible
widget markup and scoped styling without making WebGL transitive.

```text
Razor component
  -> scoped IFancyEffectRuntime
  -> packaged ES module
  -> effect adapter
       -> vendored shader.gallery renderer (ShaderBackground)
       -> IntersectionObserver (Reveal)
       -> pointer events (Tilt)
       -> CSS only (GlowBorder)
```

```text
Preview companion boundary:
Razor component -> separate companion runtime -> companion ES modules -> vendored Three.js r184
```

```text
Planned UI companion boundary:
UI component -> semantic HTML + scoped styles -> core palettes/motion defaults
                                             -> no transitive WebGL dependency
```

Components render meaningful HTML during SSR. After interactivity,
`OnAfterRenderAsync` sends create/update calls; disposal sends destroy.
JavaScript owns render frames and browser resources. The shared module stores
instances by numeric handle and may rebuild internal renderers without changing
consumer markup.

Decorative canvas/glare elements are hidden from assistive technology,
non-focusable, and pointer-transparent. Reveal never hides content from the
accessibility tree. Tilt adds no keyboard semantics. Reduced motion resolves to
static/final states. Initialization failures log once and leave fallback content.

Razor CSS isolation supplies structural/effect rules. Stable
`syntax-circus-fancy-*` classes and `--sc-fancy-*` variables are the styling
surface. Vendored files are immutable; FancyBlazor adaptations stay separate.

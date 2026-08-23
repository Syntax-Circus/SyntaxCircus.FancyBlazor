# FancyBlazor Architecture

## Topology

One Razor Class Library contains the C# API, Razor components, isolated CSS,
FancyBlazor's JavaScript adapter, and vendored shader-gallery assets. The demo
and test projects are non-packable consumers.

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

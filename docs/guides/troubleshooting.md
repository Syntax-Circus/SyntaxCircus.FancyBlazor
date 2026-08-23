# Troubleshooting

## The fallback appears but no shader

Confirm `AddFancyBlazor()` is registered in the active executable host. Check
that `_content/SyntaxCircus.FancyBlazor/js/fancy-blazor.js` and
`vendor/shader-gallery/nacre.frag` return `200`. WebGL-disabled browsers
intentionally keep the fallback.

## The component is unstyled

The host must include its generated `{HostAssembly}.styles.css`, as standard
Blazor templates do. Do not link a FancyBlazor stylesheet manually.

## Effects move despite expectations

Keep `RespectSystem` and verify the browser reports
`prefers-reduced-motion: reduce`. `IgnoreSystem` deliberately overrides it.

## Interactive Auto works only on first visit

Register `AddFancyBlazor()` in both server and client projects. Later visits may
run the component in WebAssembly instead of the server circuit.

## Content cannot be clicked

FancyBlazor decorative layers are pointer-transparent. Inspect host styles for
positioned overlays or `pointer-events` rules above the content.

## TextReveal is not animated

TextReveal intentionally accepts plain text through `Text`; it does not split
arbitrary markup. Confirm JavaScript interactivity is available and that reduced
motion is not active. The complete semantic text remains visible in either case.

## Aurora, Ripple, or CursorTrail appears static

Aurora drifts slowly by default; use a contrasting palette and a shorter
`Duration` when motion must be immediately apparent. NoiseOverlay is static by
design. Ripple appears on pointer press, but navigation can leave the page before
its short wave is visible—use an in-place button to demonstrate it. CursorTrail
appears only while a mouse or pen pointer moves over its surface. All three
respect reduced-motion preferences.

## Inspecting lifecycle state in development

Set `EnableDiagnostics = true` during registration to expose
`globalThis.__syntaxCircusFancyBlazor` in the browser. Its instance and animation
frame counts are intended for tests and local troubleshooting only; the object
is unsupported diagnostic surface and may change between previews. Diagnostics
are disabled by default and should stay disabled in production.

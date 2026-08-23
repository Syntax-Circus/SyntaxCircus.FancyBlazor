# CSS-first catalog

The CSS-first catalog is designed for static SSR and browser environments where JavaScript is unavailable. Text and surface effects preserve usable inherited content; decorative fields, texture, corners, and edge glows are hidden from assistive technology and do not intercept input.

Use `TextStroke` and `HighlightText` on short semantic text, then verify their fallback color and contrast. `GradientDivider`, `WaveDivider`, and `SectionDivider` are decorative only. `MeshBackground`, `PaperSurface`, `CornerAccents`, and `EdgeGlow` wrap ordinary content and have no animation loop, listener, observer, or JavaScript resource to clean up.

Reduced motion needs no special configuration for this collection because its default presentation is static.

[Compiling examples](../../samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)

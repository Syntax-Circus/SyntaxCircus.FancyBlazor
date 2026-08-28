# FancyBlazor WebGL Preview

`SyntaxCircus.FancyBlazor.WebGL` is the optional preview companion to
`SyntaxCircus.FancyBlazor`. It adds GPU-backed effects without placing Three.js
or its rendering lifecycle in the core package.

> **Preview API.** Components, parameters, defaults, and visual output may
> change before 1.0. Use the CSS fallback as part of the design, not as an
> exceptional state.

## Install and register

Install the companion; NuGet brings in the matching core dependency.

```bash
dotnet add package SyntaxCircus.FancyBlazor.WebGL
```

Register it in every executable host. Interactive Auto applications register
it in both the server and `.Client` projects.

```csharp
using SyntaxCircus.FancyBlazor;

builder.Services.AddFancyBlazorWebGl();
```

This registers core defaults too. Call `AddFancyBlazor(...)` first if the host
needs custom shared motion, quality, pause, or diagnostics options.

No Node, npm, CDN, script tag, or manual stylesheet import is required.

## HolographicSurface

```razor
<HolographicSurface Palette="FancyPalettes.Witchlight"
                      Intensity="0.68"
                      Depth="0.42"
                      Sheen="0.74"
                      Speed="0.85"
                      Interactive>
    <article>Semantic content remains ordinary HTML.</article>
</HolographicSurface>
```

The canvas is decorative and pointer-transparent. Static SSR, reduced motion,
disabled WebGL, context limits, and renderer failures retain the semantic child
content and a palette-derived CSS treatment.

## WaveFieldBackground

```razor
<WaveFieldBackground Palette="FancyPalettes.Witchlight"
                      Intensity="0.6"
                      Amplitude="0.55"
                      Frequency="0.45"
                      Foam="0.6"
                      Speed="0.9"
                      Interactive>
    <article>Semantic content remains ordinary HTML.</article>
</WaveFieldBackground>
```

The canvas is decorative and pointer-transparent. Static SSR, reduced motion,
disabled WebGL, context limits, and renderer failures retain the semantic child
content and a palette-derived CSS treatment.

## RefractiveOrbBackground

```razor
<RefractiveOrbBackground Palette="FancyPalettes.Witchlight"
                          Intensity="0.6"
                          Radius="0.55"
                          Distortion="0.5"
                          Sheen="0.65"
                          Speed="0.8"
                          Interactive>
    <article>Semantic content remains ordinary HTML.</article>
</RefractiveOrbBackground>
```

The canvas is decorative and pointer-transparent. Static SSR, reduced motion,
disabled WebGL, context limits, and renderer failures retain the semantic child
content and a palette-derived CSS treatment.

## PrismFieldBackground

```razor
<PrismFieldBackground Palette="FancyPalettes.Witchlight"
                       Intensity="0.6"
                       Facets="0.5"
                       Dispersion="0.5"
                       Sheen="0.6"
                       Speed="0.7"
                       Interactive>
    <article>Semantic content remains ordinary HTML.</article>
</PrismFieldBackground>
```

The canvas is decorative and pointer-transparent. Static SSR, reduced motion,
disabled WebGL, context limits, and renderer failures retain the semantic child
content and a palette-derived CSS treatment.

## ParticleFieldBackground

```razor
<ParticleFieldBackground Palette="FancyPalettes.Witchlight"
                          Intensity="0.6"
                          Density="0.5"
                          Size="0.5"
                          Drift="0.5"
                          Speed="0.9"
                          Interactive>
    <article>Semantic content remains ordinary HTML.</article>
</ParticleFieldBackground>
```

The canvas is decorative and pointer-transparent. Static SSR, reduced motion,
disabled WebGL, context limits, and renderer failures retain the semantic child
content and a palette-derived CSS treatment.

The package vendors the unmodified official Three.js r184 ESM build under the
MIT License. FancyBlazor's visual direction was informed by the
[ThreeUI effect catalog](https://github.com/MengTo/threeui), but no ThreeUI
source code or assets are included.

See the [repository documentation](https://github.com/Syntax-Circus/SyntaxCircus.FancyBlazor)
for the live showcase, complete API table, accessibility guidance, performance
limits, provenance, and third-party notices.

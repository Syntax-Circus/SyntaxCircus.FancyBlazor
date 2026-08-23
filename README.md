# FancyBlazor

[![Build](https://github.com/Syntax-Circus/SyntaxCircus.FancyBlazor/actions/workflows/build.yml/badge.svg)](https://github.com/Syntax-Circus/SyntaxCircus.FancyBlazor/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SyntaxCircus.FancyBlazor.svg)](https://www.nuget.org/packages/SyntaxCircus.FancyBlazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

<p align="center">
  <img src="docs/assets/fancyblazor-marketing-640.png" width="320" alt="FancyBlazor optical mark" />
</p>

Composable visual effects for Blazor: backgrounds, borders, viewport reveals,
pointer motion, parallax, and shimmer—without React islands, a runtime CDN, or
consumer JavaScript tooling.

FancyBlazor targets `net10.0` and works with static SSR, Interactive Server,
Interactive WebAssembly, Interactive Auto, and standalone WebAssembly. It is
CSS-framework-agnostic and keeps meaningful child content as ordinary semantic DOM.

> **Preview software.** The API may change before 1.0. Published as-is and
> maintained on a best-effort basis; there is no support SLA.

## Install and register

```bash
dotnet add package SyntaxCircus.FancyBlazor
```

Register the shared runtime in every executable host. Interactive Auto apps
register it in both server and client `Program.cs` files.

```csharp
using SyntaxCircus.FancyBlazor;

builder.Services.AddFancyBlazor();
```

Import the root namespace in `_Imports.razor`:

```razor
@using SyntaxCircus.FancyBlazor
```

No script tag, npm package, CDN, or manual stylesheet import is required.

## First effect

```razor
<ShaderBackground Effect="ShaderEffect.Nacre"
                  Palette="FancyPalettes.Witchlight">
    <section class="hero">
        <h1>Build something that catches light.</h1>
        <p>This remains semantic HTML before and after WebGL initializes.</p>
    </section>
</ShaderBackground>
```

Before interactivity—or when WebGL is unavailable—the component displays a
palette-derived CSS background and leaves the content usable.

## Preview components

| Component | Rendering path | Purpose |
| --- | --- | --- |
| `ShaderBackground` | Vendored WebGL runtime + Nacre GLSL | Decorative animated background behind real DOM |
| `GlowBorder` | CSS-first | Animated edge light around existing content |
| `Reveal` | CSS + `IntersectionObserver` | Viewport-aware fade/translate/blur entrance |
| `Tilt` | CSS + pointer JavaScript | Perspective motion and optional glare |
| `GradientBackground` | CSS-first | Palette-derived animated gradient background |
| `Spotlight` | CSS + pointer JavaScript | Decorative pointer-following radial light |
| `Magnetic` | CSS + pointer JavaScript | Subtle pointer-relative content motion |
| `Parallax` | CSS + scroll JavaScript | Viewport-relative content offset |
| `Stagger` | CSS + `IntersectionObserver` | Sequential direct-child viewport entry |
| `Shimmer` | CSS-first | Decorative highlight sweep |
| `GradientText` | CSS-first | Palette-derived text color treatment |
| `TextReveal` | CSS + `IntersectionObserver` | Semantic word or character entrance |
| `AuroraBackground` | CSS-first | Palette-derived ambient light behind content |
| `NoiseOverlay` | CSS-first | Decorative grain behind content |
| `Ripple` | CSS + pointer JavaScript | Decorative tap/click wave around content |
| `CursorTrail` | Canvas + pointer JavaScript | Bounded decorative pointer particles |
| `GlassSurface` | CSS-first | Translucent reading plane with progressive backdrop blur |
| `BorderBeam` | CSS-first | Focused moving edge-light accent |
| `GridBackground` | CSS-first | Faded line grid behind semantic content |
| `DotPattern` | CSS-first | Faded dot field behind semantic content |
| `OrbitalGlow` | CSS-first | Palette-derived ambient orbital light |
| `ScrollScene` | CSS + scroll JavaScript | Continuous in-flow semantic section treatment |
| `ScrollIndicator` | CSS + scroll JavaScript | Decorative local reading-progress line |
| `ScrollBackdrop` | CSS + scroll JavaScript | Palette-derived local scroll backdrop |
| `HoverLift` | CSS-first | Fine-pointer hover elevation around existing content |
| `PressScale` | CSS + activation JavaScript | Pointer and keyboard press response around existing content |
| `FocusHalo` | CSS-first | Additive focus halo around focused child content |
| `TextStroke` | CSS-first | Decorative text outline around semantic content |
| `HighlightText` | CSS-first | Editorial marker wash behind semantic text |
| `GradientDivider` | CSS-first | Decorative gradient separator |
| `WaveDivider` | CSS-first | Decorative static wave separator |
| `SectionDivider` | CSS-first | Decorative centered section marker |
| `MeshBackground` | CSS-first | Palette-derived static color field behind content |
| `CornerAccents` | CSS-first | Decorative opposing corners around content |
| `PaperSurface` | CSS-first | Tinted, lightly textured reading plane |
| `EdgeGlow` | CSS-first | Focused decorative edge bloom |
| `ConstellationBackground` | Canvas 2D + JavaScript | Bounded decorative point-and-line field behind real DOM |
| `ArcFlowBackground` | Canvas 2D + JavaScript | Bounded drifting-arc field behind real DOM |
| `NeonText` | CSS-first | Semantic text glow and optional outline |
| `TypeFlow` | CSS + `IntersectionObserver` | Semantic word or character entrance |
| `StatusPulse` | CSS-first | Decorative pulse around consumer-owned content |
| `LaunchHalo` | CSS-first | Decorative launch halo around consumer-owned content |
| `AuroraHero`, `ReadingSurface`, `ActionCard`, `EditorialHero`, `FeaturePanel` | Composition presets | Named, typed decorative stacks around semantic child content |

Components intentionally compose:

```razor
<ShaderBackground>
    <Reveal Effect="RevealEffect.BlurUp">
        <Tilt Glare>
            <GlowBorder Color="currentColor">
                <article>Existing content</article>
            </GlowBorder>
        </Tilt>
    </Reveal>
</ShaderBackground>
```

## Global defaults

```csharp
builder.Services.AddFancyBlazor(options =>
{
    options.MotionPreference = FancyMotionPreference.RespectSystem;
    options.Quality = FancyQuality.Auto;
    options.PauseWhenHidden = true;
    options.PauseWhenOffscreen = true;
    options.EnableDiagnostics = false;
});
```

Reduced motion is respected by default. Decorative canvases and glare layers
are hidden from assistive technology, wrapper components add no tab stops, and
continuous rendering stops while hidden or offscreen.

## Documentation and examples

- [Getting started](docs/getting-started.md)
- [Component guides and API tables](docs/README.md#components)
- [Palettes and styling](docs/guides/palettes-and-styling.md)
- [Accessibility](docs/guides/accessibility.md)
- [Performance](docs/guides/performance.md)
- [Hosting modes](docs/guides/hosting-modes.md)
- [Deploy the interactive demo container](docs/guides/demo-container.md)
- [Troubleshooting](docs/guides/troubleshooting.md)
- [Expressive effects: what to expect](docs/guides/expressive-effects.md)
- [Expressive-effects demo](samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)
- [Spatial surfaces](docs/guides/spatial-surfaces.md)
- [Spatial-surfaces demo](samples/FancyBlazor.Demo.Client/Pages/SpatialSurfaces.razor)
- [Narrative motion](docs/guides/narrative-motion.md)
- [Narrative-motion demo](samples/FancyBlazor.Demo.Client/Pages/NarrativeMotion.razor)
- [Interaction feedback](docs/guides/interaction-feedback.md)
- [Interaction-feedback demo](samples/FancyBlazor.Demo.Client/Pages/InteractionFeedback.razor)
- [CSS-first catalog](docs/guides/css-first-catalog.md)
- [CSS-first catalog demo](samples/FancyBlazor.Demo.Client/Pages/CssFirstCatalog.razor)
- [Composition and authoring](docs/guides/composition-and-authoring.md)
- [Composition-and-authoring demo](samples/FancyBlazor.Demo.Client/Pages/CompositionAuthoring.razor)
- [Atmosphere-and-accents demo](samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)
- [Changelog](CHANGELOG.md)
- [Compiling Interactive Auto demo](samples/FancyBlazor.Demo.Client/Pages/Home.razor)

## Validation

```bash
dotnet restore SyntaxCircus.FancyBlazor.slnx
dotnet build SyntaxCircus.FancyBlazor.slnx --no-restore --configuration Release
dotnet test SyntaxCircus.FancyBlazor.slnx --no-build --configuration Release
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release
pwsh eng/verify-docs.ps1
pwsh eng/verify-package.ps1
```

Install the Playwright browser once before the first local browser-test run;
the exact command is documented in [AGENTS.md](AGENTS.md#commands).

## Contributing and license

Read [AGENTS.md](AGENTS.md) before changing public behavior or vendored assets.
FancyBlazor is MIT licensed; see [LICENSE](LICENSE). Bundled shader.gallery
notices are in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

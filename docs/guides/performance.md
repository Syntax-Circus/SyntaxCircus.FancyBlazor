# Performance

JavaScript owns every animation frame; Blazor receives no frame callbacks.
`ShaderBackground` uses `ResizeObserver`, pauses when offscreen, and tears down
its WebGL renderer when the document is hidden. `Tilt` batches pointer movement
into one animation-frame update. `Spotlight`, `Magnetic`, and `Parallax` use
passive browser events and one animation-frame update; `Reveal` and `Stagger`
use `IntersectionObserver`. Every component removes resources on disposal.
`CursorTrail` uses one capped canvas particle set and draws only while particles
remain; `Ripple` removes its short-lived decorative nodes after each wave.
ScrollScene, ScrollIndicator, and ScrollBackdrop use passive scroll/resize
events, an intersection gate, and at most one pending animation frame per
wrapper; they do not run a continuous rendering loop. PressScale owns only
short-lived activation listeners, while HoverLift and FocusHalo are CSS-first.
GridBackground, DotPattern, GlassSurface, and BorderBeam use scoped CSS only;
OrbitalGlow is a single CSS light layer. Prefer static grids and dots for dense
lists, and reserve continuously animated OrbitalGlow and BorderBeam for focal surfaces.

| Quality | Maximum shader DPR |
| --- | --- |
| `Low` | 1 |
| `Auto` / `Medium` | 1.5 |
| `High` | 2 |

Prefer one large background over many simultaneous GPU tiles. Keep
`Interactive` off unless pointer response adds value. Use `Low` for very large
mobile surfaces. Use Parallax sparingly and reserve multi-layer depth scenes
for focal content; avoid stacking many scroll-driven wrappers on long lists.

ConstellationBackground and ArcFlowBackground use bounded Canvas 2D particle or arc sets, a quality-capped DPR, `ResizeObserver`, and intersection/document-visibility gates. They have one frame at most while visible and release observers, frames, and canvas contents on disposal. NeonText, StatusPulse, and LaunchHalo are CSS-first; TypeFlow uses viewport observation only.

FlickerGrid, MeteorBackground, and LightRaysBackground share the same bounded Canvas 2D, quality-capped DPR, `ResizeObserver`, and intersection/document-visibility gating as ConstellationBackground and ArcFlowBackground. ScrambleText uses viewport observation only, staggering its character animation across at most one `requestAnimationFrame` chain. Marquee is a CSS animation whose `animation-play-state` is toggled by an `IntersectionObserver`, document visibility, and pointer hover, so it runs no JavaScript animation frame of its own. NumberTicker runs one `requestAnimationFrame` chain only while its target value is counting and stops once the final value is reached. Every one of the six releases its observers, listeners, and frames on disposal.

`WordRotate`, `MorphText`, and `Typewriter` share a single JS dispatcher lifecycle. Each instance owns one `IntersectionObserver` and at most one active `requestAnimationFrame` chain; both are released while the host is offscreen, while the document is hidden, and on disposal. There is no continuous frame loop. The visible motion uses scoped CSS transitions, and the typewriter caret blink is a CSS-only animation. As a soft guidance, treat ~12 simultaneous kinetic text instances per page as the upper bound; beyond that, prefer fewer visible effects over additional cadence.

## WebGL preview companion

`HolographicSurface`, `WaveFieldBackground`, `RefractiveOrbBackground`,
`PrismFieldBackground`, and `ParticleFieldBackground` are opt-in through
`SyntaxCircus.FancyBlazor.WebGL`, so the vendored Three.js r184 payload is
absent from core-only applications. The companion defaults to at most four
active WebGL contexts, clamps overrides from one through eight, quality-caps
DPR, pauses while hidden or offscreen, and releases frames, observers,
listeners, renderers, materials, geometry, and the GPU context on teardown.
Each effect renders a single full-screen quad with one shader pass and no
render targets or textures. Prefer one or two focal surfaces, use `Low` for
large mobile treatments, keep `Interactive` off when it adds no value, and
design the palette-derived CSS fallback as an intentional state.

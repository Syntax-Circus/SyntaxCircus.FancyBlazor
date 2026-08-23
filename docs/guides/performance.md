# Performance

JavaScript owns every animation frame; Blazor receives no frame callbacks.
`ShaderBackground` uses `ResizeObserver`, pauses when offscreen, and tears down
its WebGL renderer when the document is hidden. `Tilt` batches pointer movement
into one animation-frame update. `Spotlight`, `Magnetic`, and `Parallax` use
passive browser events and one animation-frame update; `Reveal` and `Stagger`
use `IntersectionObserver`. Every component removes resources on disposal.
`CursorTrail` uses one capped canvas particle set and draws only while particles
remain; `Ripple` removes its short-lived decorative nodes after each wave.

| Quality | Maximum shader DPR |
| --- | --- |
| `Low` | 1 |
| `Auto` / `Medium` | 1.5 |
| `High` | 2 |

Prefer one large background over many simultaneous GPU tiles. Keep
`Interactive` off unless pointer response adds value. Use `Low` for very large
mobile surfaces. Use Parallax sparingly and reserve multi-layer depth scenes
for focal content; avoid stacking many scroll-driven wrappers on long lists.

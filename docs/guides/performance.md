# Performance

JavaScript owns every animation frame; Blazor receives no frame callbacks.
`ShaderBackground` uses `ResizeObserver`, pauses when offscreen, and tears down
its WebGL renderer when the document is hidden. `Tilt` batches pointer movement
into one animation-frame update. Every component removes resources on disposal.

| Quality | Maximum shader DPR |
| --- | --- |
| `Low` | 1 |
| `Auto` / `Medium` | 1.5 |
| `High` | 2 |

Prefer one large background over many simultaneous GPU tiles. Keep
`Interactive` off unless pointer response adds value. Use `Low` for very large
mobile surfaces.

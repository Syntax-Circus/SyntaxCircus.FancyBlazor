# CursorTrail

`CursorTrail` draws a bounded, decorative canvas trail behind pointer movement.

```razor
<CursorTrail Color="#a7f3d0"><section>Semantic content</section></CursorTrail>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Color` | `currentColor` | Particle color. |
| `Size` | `16` | Particle size in pixels, clamped to `4..64`. |
| `ParticleCount` | `16` | Live-particle cap, clamped to `1..48`. |
| `Duration` | `450 ms` | Nonnegative particle lifetime. |
| `Disabled` | `false` | Suppresses JavaScript enhancement. |

The canvas is decorative, pointer-transparent, and cleared on reduced motion, hidden documents, and disposal. You see particles only while a mouse or pen pointer moves over the surface; they fade after `Duration` and do not persist on touch-only interaction.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)

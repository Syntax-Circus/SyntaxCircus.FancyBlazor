# ScrollVelocity

`ScrollVelocity` blurs and tints semantic content in proportion to how fast the page is currently scrolling, settling back to its resting state a moment after scrolling stops.

```razor
<ScrollVelocity Palette="FancyPalettes.Witchlight" Sensitivity="1.2">
    <h2>Faster scrolling, more motion.</h2>
</ScrollVelocity>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Palette` | `Witchlight` | Provides the accent color used by the tint. |
| `Sensitivity` | `1.5` | Higher values require faster scrolling to reach full effect strength, clamped to `.1..10`. |
| `Disabled` | `false` | Suppresses the reactive enhancement. |

The scroll listener only runs while the element is on screen and is fully self-contained — it computes its own scroll-speed signal and reacts only on itself via `--sc-fancy-scroll-velocity`/`--sc-fancy-scroll-direction` CSS custom properties, following the same pattern as `ScrollScene`, `ScrollBackdrop`, and `ScrollIndicator`. It settles to a resting, undistorted state when `prefers-reduced-motion` is set, and it releases its scroll and `IntersectionObserver` listeners on disposal.

A cross-component variant — where other effects could subscribe to a shared scroll-velocity signal instead of each computing their own — was considered and deferred; see the roadmap's evaluation bank.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/InteractionScrollShowcase.razor)

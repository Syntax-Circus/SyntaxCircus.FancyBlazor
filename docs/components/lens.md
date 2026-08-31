# Lens

`Lens` shows a decorative, pointer-following magnified view of a background image over semantic child content — typically the same image at normal scale.

```razor
<Lens ImageUrl="/images/photo.jpg" Zoom="3" LensSize="180">
    <img src="/images/photo.jpg" alt="Description" />
</Lens>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `ImageUrl` | *(required)* | The image magnified inside the lens. |
| `Zoom` | `2.5` | Magnification factor, clamped to `1.5..5`. |
| `LensSize` | `160` | Lens diameter in pixels, clamped to `60..480`. |
| `Disabled` | `false` | Suppresses the pointer-following enhancement. |

`Lens` reuses the same pointer-tracking pattern as `Spotlight` and `Tilt` — rect-relative pointer position exposed as CSS custom properties — rather than inventing new interaction machinery. It only follows the pointer; it never locks on click or captures focus, keeping it decorative rather than interactive. True magnification of arbitrary child DOM content isn't feasible in CSS, so the magnified view is always the supplied `ImageUrl`, not a live render of `ChildContent`. The lens is hidden entirely under `prefers-reduced-motion` and when `Disabled`.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/InteractionScrollShowcase.razor)

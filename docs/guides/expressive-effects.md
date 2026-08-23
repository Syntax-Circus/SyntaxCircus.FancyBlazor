# Expressive effects: what to expect

The expressive catalog is deliberately split between ambient styling and
interaction feedback. Effects enhance ordinary semantic content; they never
provide application behavior.

## Ambient surfaces

`AuroraBackground` is a continuously drifting CSS light field. Its default
motion is intentionally subtle: use a contrasting `Palette`, raise `Intensity`,
or shorten `Duration` when the movement must be evident in a focal demo. With
reduced motion, `Animated="false"`, or `Disabled="true"`, it becomes a static
palette background (or removes the decorative lights when disabled).

`NoiseOverlay` is static texture, not animation. It adds grain immediately and
does not use JavaScript; pair it with Aurora when you want surface texture over
a moving light field. Lower `Opacity` first if the texture competes with text.

`GradientText` is static unless `Animated="true"`. Its animation shifts color,
not layout or text content, and reduced motion holds the current static gradient.

`ConstellationBackground` and `ArcFlowBackground` are bounded Canvas 2D fields
for focal content, not page-wide simulation engines. They retain a palette-derived
CSS field when Canvas, JavaScript, or motion is unavailable, and pause when hidden
or offscreen. `NeonText` is a CSS-first semantic text treatment.

## Text entrances

`TextReveal` only begins after its semantic element enters the viewport. Static
SSR and reduced motion show the complete text immediately; this is intentional,
not an initialization failure. It accepts plain `Text` so it can safely build
visual word/character tokens while keeping one accessible semantic heading,
paragraph, or span. Use `ReplayToken` to replay a demonstration.

`TypeFlow` follows the same semantic and reduced-motion contract with a smaller
typed surface for upward or downward word/character flow. Its visual tokens are
decorative; its complete text remains available to assistive technology.

## Pointer feedback

`Ripple` emits one expanding, fading circle on pointer press. It is most useful
around an in-place action such as a button or selectable surface. A normal link
may navigate away before the short animation is observable; that does not mean
the link or effect is broken. Ripple does not add application behavior or delay
navigation.

`CursorTrail` is visible only while a mouse or pen pointer moves over its
surface; particles fade within `Duration`. It intentionally has no persistent
trail and is suppressed for reduced motion. Do not use it as the sole indicator
that a control is interactive.

`StatusPulse` and `LaunchHalo` are CSS-first accents for a consumer-owned button,
link, or panel. They do not announce status, add activation, or replace the child
focus outline. Supply real status text when an application state must be announced.

## Verify a demo

The [compiling expressive-effects demo](../../samples/FancyBlazor.Demo.Client/Pages/ExpressiveEffects.razor)
uses a high-contrast, nine-second Aurora so its drift is easier to see. It uses
an in-place button for Ripple so the page remains in place long enough to show
the wave. If an effect remains static unexpectedly, first check the operating
system/browser reduced-motion setting and then confirm the host registered
`AddFancyBlazor()` in its interactive executable project.

The [compiling atmosphere-and-accents demo](../../samples/FancyBlazor.Demo.Client/Pages/ThreeUiInspiration.razor)
shows the v0.2.0 Canvas fields, text treatments, and child-preserving accents.

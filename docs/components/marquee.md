# Marquee

`Marquee` scrolls duplicated child content in a seamless decorative loop while keeping exactly one copy accessible.

```razor
<Marquee Duration="TimeSpan.FromSeconds(24)" PauseOnHover="true">
    <span>Announcement one</span>
    <span>Announcement two</span>
</Marquee>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Duration` | `20s` | Nonnegative time for one full loop. |
| `Reverse` | `false` | Reverses scroll direction. |
| `PauseOnHover` | `false` | Pauses the loop while pointer-hovered. |
| `Disabled` | `false` | Renders one static, unscrolled copy. |

Content renders twice for a seamless loop: the first copy is real and accessible, the second is `aria-hidden` and `inert` so it is never announced or keyboard-reachable. The loop pauses while offscreen, hidden, or reduced motion is preferred, and releases its observers and listeners on disposal.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/CoreKineticCatalog.razor)

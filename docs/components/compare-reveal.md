# CompareReveal

`CompareReveal` reveals one of two pieces of content against the other by dragging a handle, or by focusing it and using the arrow keys.

```razor
<CompareReveal BeforeLabel="Muted" AfterLabel="Vivid" SnapPoints="@(new[] { 0d, 50d, 100d })">
    <Before><img src="/images/before.jpg" alt="Before" /></Before>
    <After><img src="/images/after.jpg" alt="After" /></After>
</CompareReveal>
```

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Before` | *(required)* | Content revealed as the handle moves toward `After`. |
| `After` | *(required)* | Content underneath, revealed as the handle moves toward `Before`. |
| `Orientation` | `Horizontal` | `Horizontal` or `Vertical` handle axis. |
| `InitialPosition` | `50` | Starting reveal percentage, clamped to `0..100`. |
| `BeforeLabel` / `AfterLabel` | `null` | Optional caption chips over each side. |
| `SnapPoints` | `null` | Optional list of percentages the handle snaps to on release. |
| `AriaLabel` | `"Comparison position"` | Accessible name for the drag control. |
| `Disabled` | `false` | Disables the drag control and stops live updates. |

The actual control is a native `<input type="range">`, restyled to cover the whole track invisibly — so it carries proper `role="slider"`/`aria-valuenow` semantics, a real focus ring, and native `Home`/`End` handling for free. Pointer dragging and (for `Vertical`) the arrow keys are driven directly from pointer/keyboard position rather than the browser's own slider geometry, since native vertical range inputs are notoriously inconsistent about which edge is the minimum across browsers — this keeps dragging and arrow keys matching the direction you'd expect regardless of orientation. `Before` and `After` are ordinary semantic content, not images specifically; only the currently-revealed side of `Before` is visually clipped via `clip-path`, so both stay in the accessibility tree. The initial reveal percentage renders correctly before any JavaScript runs.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/InteractionScrollShowcase.razor)

# Testimonial

`Testimonial` renders a themed, non-interactive `<figure>`/`<blockquote>` for
a single customer or user quote, with an optional attribution and avatar.

```razor
<Testimonial>
    <ChildContent><p>FancyBlazor let us ship a landing page without adopting a whole design system.</p></ChildContent>
    <Attribution><cite>Jane Doe</cite>, CEO of Acme</Attribution>
    <Avatar><img src="/avatars/jane.jpg" alt="" /></Avatar>
</Testimonial>
```

`Attribution` and `Avatar` are named `RenderFragment` parameters; because
`Testimonial` declares them alongside `ChildContent`, wrap the quote body in
an explicit `<ChildContent>` tag whenever you also set `Attribution` or
`Avatar`. Wrap the person's name in `<cite>` yourself if the attribution
names a source, per native `<blockquote>`/citation semantics.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `ChildContent` | — | Quoted text, rendered inside `<blockquote>` (required). |
| `Attribution` | — | Optional attribution content, rendered inside `<figcaption>`. Omitted from the DOM when not provided. |
| `Avatar` | — | Optional avatar/logo content shown alongside the attribution. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

# FancyBlazor UI Companion

`SyntaxCircus.FancyBlazor.UI` is the optional styled, accessible UI companion
to `SyntaxCircus.FancyBlazor`. It adds semantic widgets—buttons, links,
badges, cards, and a navbar—without placing widget semantics in the core
package or making the WebGL companion transitive.

> **Preview API.** Components, parameters, defaults, and visual output may
> change before 1.0.

## Install and register

Install the companion; NuGet brings in the matching core dependency.

```bash
dotnet add package SyntaxCircus.FancyBlazor.UI
```

Register it in every executable host. Interactive Auto applications register
it in both the server and `.Client` projects.

```csharp
using SyntaxCircus.FancyBlazor;

builder.Services.AddFancyBlazorUi();
```

This registers core defaults too. Call `AddFancyBlazor(...)` first if the host
needs custom shared motion, quality, pause, or diagnostics options.

No Node, npm, CDN, script tag, or manual stylesheet import is required. The
controls coexist cleanly with Bootstrap 5's Reboot and other CSS frameworks:
they never emit framework classes or rely on inherited styling.

## FancyButton

```razor
<FancyButton Type="submit" OnClick="Save">Save changes</FancyButton>
```

Renders a native `<button>`; keyboard operability comes from the element
itself. `Disabled` sets the native `disabled` attribute and suppresses
`OnClick`.

## FancyLink

```razor
<FancyLink Href="/details">Open details</FancyLink>
```

Renders a native `<a>`. `Target="_blank"` automatically adds
`rel="noopener noreferrer"` unless you supply your own `rel`. `Disabled`
omits `href` and sets `aria-disabled="true"`.

## FancyBadge

```razor
<FancyBadge>New</FancyBadge>
```

Renders a themed, non-interactive `<span>` status label.

## FancyCard

```razor
<FancyCard>
    <Header>Plan: Pro</Header>
    <ChildContent><p>Everything in Free, plus priority support.</p></ChildContent>
    <Footer><FancyButton>Choose plan</FancyButton></Footer>
</FancyCard>
```

Renders a themed, non-interactive `<article>`. `Header` and `Footer` are
optional and omitted from the DOM when not provided; wrap the body in
`<ChildContent>` whenever you also set `Header` or `Footer`.

## FancyNavbar

```razor
<FancyNavbar AriaLabel="Site">
    <Brand><FancyLink Href="/">Acme</FancyLink></Brand>
    <Links>
        <FancyLink Href="/pricing">Pricing</FancyLink>
        <FancyLink Href="/docs">Docs</FancyLink>
    </Links>
    <Actions><FancyButton>Sign in</FancyButton></Actions>
</FancyNavbar>
```

Renders a themed `<nav>` landmark. `Brand`, `Links`, and `Actions` are
optional slots omitted from the DOM when not provided. No built-in mobile
disclosure — keyboard navigation is native tab order across whatever
interactive elements you place in the slots.

See the [repository documentation](https://github.com/Syntax-Circus/SyntaxCircus.FancyBlazor)
for the live showcase, complete API table, accessibility guidance, and
third-party notices.

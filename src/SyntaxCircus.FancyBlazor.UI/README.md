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

## LogoCloud

```razor
<LogoCloud AriaLabel="Trusted by">
    <li><img src="/logos/acme.svg" alt="Acme" /></li>
    <li><img src="/logos/globex.svg" alt="Globex" /></li>
</LogoCloud>
```

Renders a themed, non-interactive `<ul>` list for consumer-supplied `<li>`
logo content. `Layout` is `Wrap` (default) or `Dense`.

## Testimonial

```razor
<Testimonial>
    <ChildContent><p>FancyBlazor let us ship a landing page without adopting a whole design system.</p></ChildContent>
    <Attribution><cite>Jane Doe</cite>, CEO of Acme</Attribution>
</Testimonial>
```

Renders a themed, non-interactive `<figure>`/`<blockquote>` with optional
`Attribution` and `Avatar` slots, omitted from the DOM when not provided.

## CallToAction

```razor
<CallToAction Layout="CallToActionLayout.Stacked">
    <Heading><h2>Ready to try FancyBlazor?</h2></Heading>
    <ChildContent>Start building your first effect in minutes.</ChildContent>
    <Actions><FancyLink Href="/getting-started">Get started</FancyLink></Actions>
</CallToAction>
```

Renders a themed, non-interactive `<div>` with optional `Heading`,
`ChildContent`, and `Actions` slots. `Layout` is `Inline` (default) or
`Stacked`. Does not choose a heading level for you.

## FeatureGrid

```razor
<FeatureGrid AriaLabel="Features">
    <li><h3>Fast</h3><p>CSS-first effects with no required JavaScript.</p></li>
    <li><h3>Accessible</h3><p>Reduced motion by default.</p></li>
</FeatureGrid>
```

Renders a themed, non-interactive `<ul>` responsive grid for consumer-supplied
`<li>` content. `Columns` is `Two`, `Three` (default), or `Four`.

## Hero

```razor
<Hero Alignment="HeroAlignment.Center">
    <Heading><h1>Composable effects for Blazor.</h1></Heading>
    <Subheading>Ship expressive UI with ordinary semantic HTML at the center.</Subheading>
    <Actions><FancyLink Href="/ui-companion">Get started</FancyLink></Actions>
</Hero>
```

Renders a themed, non-interactive `<div>` with optional `Heading`,
`Subheading`, `Actions`, and `Background` slots. `Background` renders
`aria-hidden` and pointer-transparent; `Hero` never requires WebGL or core
renderer internals. `Alignment` is `Start` (default) or `Center`.

## PricingTable

```razor
<PricingTable AriaLabel="Plans">
    <thead>
        <tr><th scope="col">Feature</th><th scope="col">Free</th><th scope="col">Pro</th></tr>
    </thead>
    <tbody>
        <tr><th scope="row">Projects</th><td>3</td><td>Unlimited</td></tr>
    </tbody>
</PricingTable>
```

Renders a themed, non-interactive `<table>` for consumer-supplied
`<thead>`/`<tbody>`/`<tfoot>` content. `Density` is `Comfortable` (default) or
`Compact`. Set `aria-current="true"` on a plan's header/cells to mark a
featured tier; omit it and the table works the same with none featured.

## FaqAccordion

```razor
<FaqAccordion SingleOpen="true">
    <FaqAccordionItem DefaultOpen="true">
        <Question>What is FancyBlazor?</Question>
        <Answer>A visual-effects and UI component library for Blazor.</Answer>
    </FaqAccordionItem>
    <FaqAccordionItem>
        <Question>Does it require WebGL?</Question>
        <Answer>No, core and UI never require the WebGL companion.</Answer>
    </FaqAccordionItem>
</FaqAccordion>
```

Renders a keyboard-operable disclosure list from `FaqAccordionItem` children,
each a `<button>` trigger paired with a `role="region"` answer panel.
`SingleOpen` (default `true`) closes other items when one opens. This is the
**only** control in the UI companion with a JavaScript lifecycle: a small
module owns click-driven open/closed state and cleans up its listeners on
disposal. Every other control in this package remains JavaScript-free.

Renders a themed `<nav>` landmark. `Brand`, `Links`, and `Actions` are
optional slots omitted from the DOM when not provided. No built-in mobile
disclosure — keyboard navigation is native tab order across whatever
interactive elements you place in the slots.

See the [repository documentation](https://github.com/Syntax-Circus/SyntaxCircus.FancyBlazor)
for the live showcase, complete API table, accessibility guidance, and
third-party notices.

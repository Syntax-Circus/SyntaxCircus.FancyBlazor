# FaqAccordion

`FaqAccordion` renders a themed, keyboard-operable disclosure list of
questions and answers, composed from `FaqAccordionItem` children.

```razor
<FaqAccordion SingleOpen="true">
    <FaqAccordionItem DefaultOpen="true">
        <Question>What is FancyBlazor?</Question>
        <Answer>A visual-effects and UI component library for Blazor.</Answer>
    </FaqAccordionItem>
    <FaqAccordionItem>
        <Question>Does it require WebGL?</Question>
        <Answer>No, the core and UI packages never require the WebGL companion.</Answer>
    </FaqAccordionItem>
</FaqAccordion>
```

Each `FaqAccordionItem` renders a native `<button>` trigger
(`aria-expanded`/`aria-controls`) inside an `<h3>`, and a linked
`role="region"` answer panel with matching, auto-generated ids. Triggers are
real `<button>` elements, so Enter/Space activation and the tab order come
from the browser for free.

Unlike the rest of the UI companion, `FaqAccordion` uses a small JavaScript
module (`faq-accordion.js`) to own open/closed state and enforce
single-open-at-a-time behavior — it is the only control in this package that
does. The module attaches a click listener per trigger on mount and removes
every listener on disposal; it never sends per-frame updates back to Blazor.

`FaqAccordion` fills the width of its container by default, so its layout
stays stable as items expand and collapse even inside a centering flex or
grid parent.

| Parameter | Default | Behavior |
| --- | --- | --- |
| `ChildContent` | — | One or more `FaqAccordionItem` children (required). |
| `SingleOpen` | `true` | When `true`, opening an item closes the others. Set `false` to allow multiple items open at once. |
| `Animated` | `false` | When `true`, animates each panel's open/close with a CSS height transition instead of toggling instantly. Respects `prefers-reduced-motion`. |
| `Theme` | Configured default | `FancyUiTheme` tokens; falls back to `AddFancyBlazorUi()`'s configured theme. |

`FaqAccordionItem` parameters:

| Parameter | Default | Behavior |
| --- | --- | --- |
| `Question` | — | Content rendered inside the trigger `<button>` (required). |
| `Answer` | — | Content rendered inside the answer panel (required). |
| `DefaultOpen` | `false` | Whether the item starts expanded. |

Coexists with Bootstrap 5's Reboot; does not require or emit Bootstrap classes.

[Compiling example](../../samples/FancyBlazor.Demo.Client/Pages/Marketing.razor)

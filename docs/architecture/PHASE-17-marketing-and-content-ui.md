# PHASE-17: Marketing and Content UI

## Objective

Expand the UI companion with reusable marketing and editorial controls built
from consumer-owned content and the Phase 16 theme/primitives contracts.

## Committed components

- `Hero`
- `CallToAction`
- `FeatureGrid`
- `LogoCloud`
- `Testimonial`
- `PricingTable`
- `FaqAccordion`

## Actionable tasks

- [ ] Define named child fragments and typed presentation options for all seven
  controls without embedding product copy, navigation policy, or business logic.
- [ ] Preserve heading structure, landmarks, links, lists, tables, and disclosure
  semantics; `FaqAccordion` must support keyboard operation and explicit state.
- [ ] Compose core effects and Phase 16 primitives without requiring WebGL or
  exposing core renderer internals through UI APIs.
- [ ] Add a compiling marketing showcase, component guides, copy/semantics
  guidance, responsive examples, changelog entries, and contributor rules.
- [ ] Add bUnit and browser coverage for fragments, semantics, responsive
  behavior, keyboard interaction, focus, reduced motion, and nested composition.

## Success criteria

Consumers can build an expressive marketing or content page from accessible,
slot-driven controls while retaining ownership of copy, links, hierarchy, and
application behavior.

## Validation gate

Run all Release, browser, accessibility, documentation, package-content,
clean-consumer, and same-version release-set gates for the three-package catalog.

# Bootstrap 5 compatibility

`SyntaxCircus.FancyBlazor.UI` controls are verified to coexist cleanly with
Bootstrap 5's Reboot and utility CSS. Dropping `FancyButton`, `FancyLink`,
`FancyBadge`, `FancyCard`, or `FancyNavbar` into a Bootstrap-based page does
not change their appearance, and they never emit Bootstrap classes, require
Bootstrap, or depend on Bootstrap JavaScript.

## How coexistence works

Every control's scoped CSS self-declares every presentational property
Bootstrap 5's Reboot touches for its element type — `box-sizing`, `color`,
`background-color`, `border`, `text-decoration`, `font`/`line-height`, and
`appearance` — using its stable `syntax-circus-fancy-ui-*` hook's own
specificity, rather than leaving any of them to inherit from the host page or
a global element selector such as `button`, `a`, or `*`. There is no
Bootstrap detection, no `!important`, and no framework-specific code path:
the controls are simply never dependent on browser or framework defaults for
their own presentation.

## Verification

A dedicated Playwright suite renders the full catalog on a fixture page with
and without a locally vendored, unmodified Bootstrap 5 stylesheet (see
[`third-party/bootstrap/PROVENANCE.md`](../../third-party/bootstrap/PROVENANCE.md))
and asserts each control's computed background color, text color, text
decoration, and border radius are identical in both cases. That stylesheet
is test/demo-only: it is never packed into the `SyntaxCircus.FancyBlazor.UI`
NuGet package, which `eng/verify-ui-package.ps1` enforces.

The live demo's [`/ui-companion`](https://fancyblazor-demo-latest.onrender.com/ui-companion)
route includes a Bootstrap coexistence section that loads the same
stylesheet for a visual side-by-side.

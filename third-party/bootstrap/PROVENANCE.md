# Bootstrap provenance

`SyntaxCircus.FancyBlazor.UI` is verified to coexist cleanly with Bootstrap
5's Reboot and utility CSS: none of its controls emit Bootstrap classes,
require Bootstrap, or depend on Bootstrap JavaScript. To prove that
coexistence, this repository vendors one unmodified official Bootstrap 5
release build for test and demo use only.

| Local file | Source | SHA-256 |
| --- | --- | --- |
| `third-party/bootstrap/bootstrap.min.css` | `https://raw.githubusercontent.com/twbs/bootstrap/v5.3.3/dist/css/bootstrap.min.css` | `3C8F27E6009CCFD710A905E6DCF12D0EE3C6F2AC7DA05B0572D3E0D12E736FC8` |
| `licenses/bootstrap-LICENSE` | `https://raw.githubusercontent.com/twbs/bootstrap/v5.3.3/LICENSE` | `8C14611AE41AC6FD543C13349F22188EB12C69B3E59105C5ECA3925A8E4ECA3E` |

This file is **test/demo-only**. It is linked into
`tests/SyntaxCircus.FancyBlazor.TestHost` (for the Bootstrap coexistence
Playwright coverage) and `samples/FancyBlazor.Demo.Client` (for the
`/ui-companion` showcase's Bootstrap coexistence section) as a build-time
MSBuild link, not copied. `eng/verify-ui-package.ps1` asserts the packed
`SyntaxCircus.FancyBlazor.UI` NuGet package never contains a Bootstrap asset.

No changes have been made to the vendored Bootstrap file.

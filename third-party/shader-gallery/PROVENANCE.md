# shader.gallery provenance

Retrieved on 2026-08-22 using the official npm registry and CLI.

| Artifact | Identity | SHA-256 / integrity |
| --- | --- | --- |
| `shader-gallery` CLI archive | `shader-gallery@0.1.0` | SHA-256 `A284D2CC3FBD1CEE5FE04C3860554CE5D731DC53A1609DB0CDAEAAAF309675A1`; npm SHA-512 `MZ4yLWLZG2wnfxmUnMjpYTbzE0k8xw8J8of5KIzk7d8GiDl9kitUcgC1XtW1+qky2cgeWuuP1mUIhpgE90CKJw==` |
| Nacre fragment fetched by CLI | `npx shader-gallery add nacre` | SHA-256 `A0349A9BF1889F4E724EFA448D360FD50E7C85840485EEFF8CE117D6E1D74249` |
| Runtime archive | `@shader-gallery/runtime@0.1.0` | SHA-256 `E0E57DC1CDD7D3FB57AF19FF4D9596F002FAFFA4AE2818F1EB29CFE845D4C0CE`; npm SHA-512 `RAUBFNi2VGiTOqcntVvZLBGh6b7Mxhtvayg1yuV5ncQDdZbskU5vY661XwEAP8xQmgM+v06Q6i8anmVuh8Ocsw==` |
| Upstream `renderer.js` | File from runtime archive | SHA-256 `4C18CC9B9AA6705ACACE998E239AD56286B96C9835CBFAE42C4A57104AB30C20` |
| Upstream MIT license | File from CLI archive | SHA-256 `3135A55D2C5518397288163AFBD775551DC4C7EF09DF99FD68CB4C3EB0BA6736` |

Sources:

- <https://github.com/shader-gallery/shaders>
- <https://github.com/shader-gallery/runtime>
- <https://www.npmjs.com/package/shader-gallery>
- <https://www.npmjs.com/package/@shader-gallery/runtime>

`third-party/shader-gallery/renderer-0.1.0.js` is the untouched upstream source.
The packaged `wwwroot/js/shader-gallery-renderer.js` retains that source and adds
clearly marked FancyBlazor adaptations: an option allowing offscreen pausing to
be disabled and an internal read-only RAF-state diagnostic used by browser tests.
The Nacre fragment is packaged unchanged.

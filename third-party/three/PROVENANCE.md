# Three.js provenance

FancyBlazor's optional WebGL companion vendors the unmodified official Three.js r184 ESM build files.

| Local file | Source | SHA-256 |
| --- | --- | --- |
| `src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/build/three.module.js` | `https://raw.githubusercontent.com/mrdoob/three.js/r184/build/three.module.js` | `61134198639A10885DAF893FB29669CA26386E2A4CDE76E8399F51E329F741F2` |
| `src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/build/three.core.js` | `https://raw.githubusercontent.com/mrdoob/three.js/r184/build/three.core.js` | `368DC78835287709A48939E8EB9A7A61D0732098BDF916E56840D458AAE9CCF3` |
| `src/SyntaxCircus.FancyBlazor.WebGL/wwwroot/vendor/three/LICENSE` | `https://raw.githubusercontent.com/mrdoob/three.js/r184/LICENSE` | `8B378EBE60E2FE500158CB0AC71CB5E8B7D92953C2ABCC63A0EB90499653B5BC` |

The companion's adapter and renderer files are FancyBlazor code; no changes have been made to the vendored Three.js files.

ThreeUI informed the WebGL preview's visual direction only. FancyBlazor does
not include ThreeUI source code, shaders, or assets; its typed component,
renderer, lifecycle, and fallback implementation are independent.

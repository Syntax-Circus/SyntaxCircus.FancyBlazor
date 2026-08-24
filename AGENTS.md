# AGENTS.md

Guidance for contributors and coding agents working in
`SyntaxCircus.FancyBlazor`. Read [README.md](README.md) first: it is the
consumer-facing contract. Architecture decisions and phase status live in
[docs/architecture/00-DISCOVERY-INDEX.md](docs/architecture/00-DISCOVERY-INDEX.md).

## Purpose and boundary

This repository publishes a core `net10.0` Razor Class Library plus an optional,
same-version WebGL preview companion containing visual effects for Blazor. The
core preview catalog includes shader and gradient backgrounds,
glow and shimmer surfaces, reveal and stagger entrances, and pointer/scroll
motion effects, including semantic text entrances, ambient overlays, bounded
pointer particles, CSS-first spatial surfaces, in-flow narrative motion, and additive interaction feedback. FancyBlazor owns effect markup, scoped styles, coarse
Blazor-to-JavaScript lifecycle calls, and the JavaScript rendering loop.

It is not a general UI framework. Do not add buttons, inputs, layouts, routing,
middleware, authentication, SEO policy, a CSS framework, global resets, host
typography, or product-specific content. Meaningful consumer content must remain
semantic DOM; canvas and glare layers are decorative.

Use the relevant `SyntaxCircus.*` shared package for common host concerns rather
than reimplementing them in an application. In particular, demo-host crawler
discovery uses `SyntaxCircus.AspNetCore.Common`'s `MapRobotsTxt` and
`MapSitemap` endpoints, not hand-authored static files.

## Repository map

```text
src/SyntaxCircus.FancyBlazor/              package source and static assets
src/SyntaxCircus.FancyBlazor.WebGL/        optional published WebGL preview companion
samples/FancyBlazor.Demo*/                 compiling Interactive Auto demo
tests/SyntaxCircus.FancyBlazor.Tests/      xUnit, Shouldly, and bUnit contracts
tests/SyntaxCircus.FancyBlazor.BrowserTests/ Playwright lifecycle tests
docs/                                      user and architecture documentation
licenses/                                  third-party license texts
```

## Commands

Run from the repository root:

```bash
dotnet restore SyntaxCircus.FancyBlazor.slnx
dotnet build SyntaxCircus.FancyBlazor.slnx --no-restore --configuration Release
dotnet test SyntaxCircus.FancyBlazor.slnx --no-build --configuration Release
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts/release-preview
dotnet pack src/SyntaxCircus.FancyBlazor.WebGL/SyntaxCircus.FancyBlazor.WebGL.csproj --no-build --configuration Release --output artifacts/release-preview
pwsh eng/verify-docs.ps1
pwsh eng/verify-package.ps1 -PackageDirectory artifacts/release-preview
pwsh eng/verify-webgl-package.ps1 -PackageDirectory artifacts/release-preview -CorePackageDirectory artifacts/release-preview
pwsh eng/verify-release-packages.ps1 -PackageDirectory artifacts/release-preview
pwsh eng/tests/publish-nuget-packages.tests.ps1
docker build --file samples/FancyBlazor.Demo/Dockerfile --tag fancyblazor-demo:local .
```

Install Playwright browsers once before running browser tests locally:

```bash
pwsh tests/SyntaxCircus.FancyBlazor.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium
```

GitVersion derives the library's version from Git history. Do not hand-edit it.
Dependency versions belong only in `Directory.Packages.props`. If a sandbox's
repository-ownership check prevents local GitVersion evaluation, validate the
package with an explicit disposable preview version:

```bash
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts -p:DisableGitVersionTask=true -p:PackageVersion=0.1.0-preview.1
```

That override is for local validation only; CI and releases must use GitVersion.

The WebGL companion is a published preview and must use the exact core package
version. To validate locally, pack both packages to a clean
`artifacts/release-preview` directory with the same disposable version, run
both content verifiers, then run the release-set
verifier. CI derives both versions from the same Git history and publishes the
pair through one main-branch release job.

Before the companion's first NuGet release, confirm the trusted-publishing
policy owner represented by `NUGET_USER` is the intended owner of the new
`SyntaxCircus.FancyBlazor.WebGL` package ID and is authorized to create it. Keep
the `release` GitHub environment, `build.yml` policy identity, and
`id-token: write` permission aligned with the NuGet trusted-publishing policy.

```bash
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release --output artifacts/release-preview -p:DisableGitVersionTask=true -p:PackageVersion=0.2.1-preview.1
dotnet pack src/SyntaxCircus.FancyBlazor.WebGL/SyntaxCircus.FancyBlazor.WebGL.csproj --no-build --configuration Release --output artifacts/release-preview -p:DisableGitVersionTask=true -p:PackageVersion=0.2.1-preview.1
pwsh eng/verify-package.ps1 -PackageDirectory artifacts/release-preview -PackageVersion 0.2.1-preview.1
pwsh eng/verify-webgl-package.ps1 -PackageDirectory artifacts/release-preview -CorePackageDirectory artifacts/release-preview -PackageVersion 0.2.1-preview.1
pwsh eng/verify-release-packages.ps1 -PackageDirectory artifacts/release-preview
```

The demo image is published only by the main-branch workflow as
`ghcr.io/syntax-circus/fancyblazor-demo`, tagged with `latest` and the full
commit SHA. Keep the Dockerfile's publish command independent of Git history,
run the image as non-root on port `8080`, and do not add TLS or proxy policy to
the demo application.

## Public API rules

Every public component, parameter, enum value, default, rendered hook, CSS
custom property, and setup step is consumer API.

- Keep public types in `SyntaxCircus.FancyBlazor` so one Razor import is enough.
- Prefer typed C# parameters; do not expose renderer names, raw uniforms,
  shader-gallery slugs, runtime handles, or provider internals.
- Use `TimeSpan` for durations and clamp unsafe numeric inputs.
- Merge `CssClass`, `Style`, and unmatched `class`/`style` attributes without
  dropping the stable `syntax-circus-fancy-*` hook.
- Preserve child semantics. Decorative elements are `aria-hidden`, unfocusable,
  and pointer-transparent.
- Reduced motion must produce a useful static or final state by default.
- Decorative failures log once and retain usable content; they do not become
  application-fatal exceptions.

When public behavior changes, update the README, applicable user guide,
compiling demo, bUnit/browser coverage, `CHANGELOG.md`, and this file in the
same change. Keep unreleased entries under the correct Keep a Changelog
category, unless the merge is release-bound; in that case write directly to its
dated GitVersion package-version section.

## JavaScript and CSS rules

- JavaScript owns animation frames, pointer tracking, observers, WebGL objects,
  and cleanup. Never send frame updates through `IJSRuntime`.
- Components send only create, coarse update, pause/resume, and destroy calls.
- Every listener, observer, timer, frame, and WebGL resource must be released.
- Components initialize after interactivity and remain valid during static SSR.
- Ship effect-owned styles through Razor CSS isolation. Do not add global
  element selectors or assume Bootstrap, Tailwind, MudBlazor, Radzen, or Fluent.
- Default visuals should be subtle and overridable through documented CSS
  custom properties.
- Scroll effects must stay in normal document flow, use passive/event-batched
  progress work only while intersecting, and release pending frames while
  hidden, offscreen, disabled, or disposed.
- Hover effects must target fine pointers only. Press and focus treatments must
  preserve child activation and the native focus outline; reduced motion removes
  decorative transforms without removing useful focus visibility.
- CSS-first surface effects must retain a useful static fallback when an
  optional CSS feature such as `backdrop-filter` or masking is unavailable.
- CSS-first typography, divider, and surface effects must require no JavaScript
  lifecycle work. Decorative dividers are `aria-hidden`; consumers retain native
  `<hr>` semantics when a thematic break must be announced.
- Named composition presets may combine existing effects but must expose only
  typed, stable presentation controls and child content. They never provide
  roles, tab stops, click handlers, or consumer content.

## Third-party assets

`Nacre` and the shader.gallery renderer are vendored MIT assets. Do not edit
files under `wwwroot/vendor/shader-gallery` directly. Update them by repeating
the documented intake process, review the diff, update SHA-256 values in
`PROVENANCE.md`, and preserve `licenses/shader-gallery-LICENSE` and
`THIRD-PARTY-NOTICES.md`. FancyBlazor adaptations belong outside the vendor
folder.

The WebGL preview companion separately vendors unmodified Three.js r184 ESM
assets. Its package must retain `licenses/three-LICENSE` and
`third-party/three/PROVENANCE.md`, ship no Node artifact or external executable
asset load, and remain under the dedicated raw/Brotli adapter-renderer size gate.
ThreeUI is visual-direction inspiration only; do not copy or vendor its source,
shaders, or assets without a separate intake, license, and provenance decision.

## Testing and completion

- Use xUnit v3, Shouldly, and bUnit for .NET/rendering contracts.
- Use Playwright for JavaScript lifecycle, hosting mode, motion, fallback, and
  disposal behavior.
- Browser tests launch compiled test-host assemblies rather than paths in the
  source checkout so they remain valid in NCrunch workspaces. The server host
  copies its own and FancyBlazor's static assets to its output so they never
  resolve through NCrunch source-workspace paths. NCrunch ignores this project
  completely because its spawned Kestrel/Chromium integration coverage is owned
  by the regular test runner and CI. Do not add framework-level skip attributes;
  those would suppress the coverage everywhere.
- Add a test for every new or changed public rendering behavior.
- Text effects that split content must keep a semantic element with the complete
  accessible text; visual tokens are decorative only.
- Pointer-generated DOM and canvas effects must cap their transient work and
  clear it for reduced motion, hidden documents, and disposal.
- Canvas 2D background effects must provide a useful CSS fallback, cap particle
  or arc counts, quality-cap DPR, pause while hidden or offscreen, and release
  their frame, resize, intersection, and media-query resources on disposal.
- Decorative status and launch accents never announce application state, add
  activation behavior, replace native focus visibility, or own child semantics.
- New public components require a compiling demo example, user component guide,
  README catalog entry, relevant accessibility/performance guidance, changelog
  entry, and this contributor contract update in the same release-bound change.
- Add browser coverage for new scroll progress, hidden/offscreen cleanup,
  fine-pointer gating, keyboard press behavior, and additive focus feedback.
- For new CSS-first effects, add browser coverage that confirms static rendering,
  reduced-motion usability, feature fallback, and zero effect-runtime entries.
- Composition presets require bUnit coverage for nested stable hooks and child
  semantics, plus browser coverage for any included interactive behavior.
- Keep documentation snippets linked to compiling sample components.
- Run restore, Release build, all tests, browser tests, pack, both package-content
  inspections, and the same-version release-set verifier before declaring completion.
- Confirm each clean package consumer needs no Node, npm, CDN, manual script
  import, or project reference.

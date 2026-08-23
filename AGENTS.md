# AGENTS.md

Guidance for contributors and coding agents working in
`SyntaxCircus.FancyBlazor`. Read [README.md](README.md) first: it is the
consumer-facing contract. Architecture decisions and phase status live in
[docs/architecture/00-DISCOVERY-INDEX.md](docs/architecture/00-DISCOVERY-INDEX.md).

## Purpose and boundary

This repository produces one `net10.0` Razor Class Library containing visual
effects for Blazor. FancyBlazor owns effect markup, scoped styles, coarse
Blazor-to-JavaScript lifecycle calls, and the JavaScript rendering loop.

It is not a general UI framework. Do not add buttons, inputs, layouts, routing,
middleware, authentication, SEO policy, a CSS framework, global resets, host
typography, or product-specific content. Meaningful consumer content must remain
semantic DOM; canvas and glare layers are decorative.

## Repository map

```text
src/SyntaxCircus.FancyBlazor/              package source and static assets
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
dotnet pack src/SyntaxCircus.FancyBlazor/SyntaxCircus.FancyBlazor.csproj --no-build --configuration Release
pwsh eng/verify-docs.ps1
pwsh eng/verify-package.ps1
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
category; during release preparation, move them into the dated GitVersion
package-version section.

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

## Third-party assets

`Nacre` and the shader.gallery renderer are vendored MIT assets. Do not edit
files under `wwwroot/vendor/shader-gallery` directly. Update them by repeating
the documented intake process, review the diff, update SHA-256 values in
`PROVENANCE.md`, and preserve `licenses/shader-gallery-LICENSE` and
`THIRD-PARTY-NOTICES.md`. FancyBlazor adaptations belong outside the vendor
folder.

## Testing and completion

- Use xUnit v3, Shouldly, and bUnit for .NET/rendering contracts.
- Use Playwright for JavaScript lifecycle, hosting mode, motion, fallback, and
  disposal behavior.
- Add a test for every new or changed public rendering behavior.
- Keep documentation snippets linked to compiling sample components.
- Run restore, Release build, all tests, browser tests, pack, and package-content
  inspection before declaring completion.
- Confirm the package consumer needs no Node, npm, CDN, manual script import, or
  project reference.

# Getting Started

## 1. Install

```bash
dotnet add package SyntaxCircus.FancyBlazor
```

## 2. Register services

Add the registration in the executable host's `Program.cs`:

```csharp
using SyntaxCircus.FancyBlazor;

builder.Services.AddFancyBlazor();
```

For Interactive Auto, repeat the registration in the `.Client` project because
the component may move from the server circuit to WebAssembly on later visits.

## 3. Import and render

Add the namespace to `_Imports.razor`:

```razor
@using SyntaxCircus.FancyBlazor
```

Then wrap existing content:

```razor
<GlowBorder Color="#67e8f9" Radius="18">
    <article class="product-card">
        <h2>Existing card</h2>
        <a href="/details">View details</a>
    </article>
</GlowBorder>
```

FancyBlazor loads its JavaScript module through Blazor interop and ships effect
styles through Razor CSS isolation. Do not add a script tag or npm dependency.

## 4. Run the repository demo

```bash
dotnet run --project samples/FancyBlazor.Demo/FancyBlazor.Demo.csproj
```

The demo source is the canonical compiling example set:

- [Composed landing page](../samples/FancyBlazor.Demo.Client/Pages/Home.razor)
- [ShaderBackground](../samples/FancyBlazor.Demo.Client/Pages/Background.razor)
- [GlowBorder](../samples/FancyBlazor.Demo.Client/Pages/Border.razor)
- [Reveal](../samples/FancyBlazor.Demo.Client/Pages/RevealPage.razor)
- [Tilt](../samples/FancyBlazor.Demo.Client/Pages/TiltPage.razor)

Continue with [hosting modes](guides/hosting-modes.md) if your application uses
prerendering, WebAssembly, or Interactive Auto.

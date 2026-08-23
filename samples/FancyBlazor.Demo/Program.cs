using FancyBlazor.Demo.Client;
using FancyBlazor.Demo.Components;
using SyntaxCircus.FancyBlazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddFancyBlazor(options => options.EnableDiagnostics = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ClientAssemblyMarker).Assembly);

app.Run();

/// <summary>Marker used by browser tests to locate the demo entry point.</summary>
public partial class Program;

using SyntaxCircus.FancyBlazor;
using SyntaxCircus.FancyBlazor.TestHost.Components;

var builder = WebApplication.CreateBuilder(args);

// The test host is launched from test runners such as NCrunch, which may not
// have permission to write Windows Event Log entries. Keep diagnostics local to
// the child process streams captured by BrowserHostFixture.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFancyBlazor(options => options.EnableDiagnostics = true);
builder.Services.AddFancyBlazorWebGl(options => options.MaxActiveContexts = 2);
builder.Services.AddFancyBlazorUi();

var app = builder.Build();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>Exposes the executable host type for integration tooling.</summary>
public partial class Program;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SyntaxCircus.FancyBlazor;
using SyntaxCircus.FancyBlazor.StandaloneHost;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddFancyBlazor(options => options.EnableDiagnostics = true);
builder.Services.AddFancyBlazorWebGl();
builder.Services.AddFancyBlazorUi();
await builder.Build().RunAsync();

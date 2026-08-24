using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SyntaxCircus.FancyBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFancyBlazor(options => options.EnableDiagnostics = true);
builder.Services.AddFancyBlazorWebGl();
await builder.Build().RunAsync();

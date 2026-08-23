using FancyBlazor.Demo.Client;
using FancyBlazor.Demo.Components;
using SyntaxCircus.AspNetCore.Common;
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
app.MapRobotsTxt(_ => "User-agent: *\nAllow: /\nSitemap: https://fancyblazor.syntaxcircus.com/sitemap.xml");
app.MapSitemap(_ =>
[
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/background"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/border"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/spatial-surfaces"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/reveal"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/tilt"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/narrative-motion"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/expanded-effects"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/expressive-effects"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/interaction-feedback"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/css-first-catalog"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/composition-authoring"),
    new SitemapEntry("https://fancyblazor.syntaxcircus.com/threeui-inspiration"),
]);
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ClientAssemblyMarker).Assembly);

app.Run();

/// <summary>Marker used by browser tests to locate the demo entry point.</summary>
public partial class Program;

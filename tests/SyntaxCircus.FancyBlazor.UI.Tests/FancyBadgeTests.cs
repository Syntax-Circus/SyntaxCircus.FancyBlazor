using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyBadgeTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FancyBadge_RendersSpanWithStableHookAndThemeTokens()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyBadge>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "New")));

        var span = cut.Find("span");
        span.ClassList.ShouldContain("syntax-circus-fancy-ui-badge");
        span.TextContent.ShouldBe("New");
        (span.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-accent:");
    }

    [Fact]
    public void FancyBadge_MergesAttributesAndAppliesCustomTheme()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<FancyBadge>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Beta"))
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "status-badge")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["class"] = "test-hook",
                ["aria-label"] = "Beta feature",
            }));

        var span = cut.Find("span");
        span.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-badge status-badge test-hook");
        (span.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-accent:#f00");
        span.GetAttribute("aria-label").ShouldBe("Beta feature");
    }
}

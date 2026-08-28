using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyCardTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FancyCard_WithoutHeaderOrFooter_RendersOnlyBody()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyCard>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Body content")));

        cut.Find("article").ClassList.ShouldContain("syntax-circus-fancy-ui-card");
        cut.FindAll(".syntax-circus-fancy-ui-card__header").ShouldBeEmpty();
        cut.FindAll(".syntax-circus-fancy-ui-card__footer").ShouldBeEmpty();
        cut.Find(".syntax-circus-fancy-ui-card__body").TextContent.ShouldBe("Body content");
    }

    [Fact]
    public void FancyCard_WithHeaderAndFooter_RendersAllSlots()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyCard>(parameters => parameters
            .Add(component => component.Header, builder => builder.AddContent(0, "Title"))
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Body content"))
            .Add(component => component.Footer, builder => builder.AddContent(0, "Actions")));

        cut.Find(".syntax-circus-fancy-ui-card__header").TextContent.ShouldBe("Title");
        cut.Find(".syntax-circus-fancy-ui-card__body").TextContent.ShouldBe("Body content");
        cut.Find(".syntax-circus-fancy-ui-card__footer").TextContent.ShouldBe("Actions");
    }

    [Fact]
    public void FancyCard_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<FancyCard>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Body content"))
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "pricing-card")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var article = cut.Find("article");
        article.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-card pricing-card test-hook");
        (article.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-surface:#111");
    }
}

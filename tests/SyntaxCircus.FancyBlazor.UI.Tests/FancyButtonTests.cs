using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FancyButtonTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FancyButton_RendersNativeButtonWithDefaults()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyButton>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Save")));

        var button = cut.Find("button");
        button.GetAttribute("type").ShouldBe("button");
        button.ClassList.ShouldContain("syntax-circus-fancy-ui-button");
        button.TextContent.ShouldBe("Save");
        button.HasAttribute("disabled").ShouldBeFalse();
    }

    [Fact]
    public void FancyButton_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<FancyButton>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Go"))
            .Add(component => component.Type, "submit")
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "hero-cta")
            .Add(component => component.Style, "margin:1rem")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object>
            {
                ["class"] = "test-hook",
                ["aria-label"] = "Save changes",
            }));

        var button = cut.Find("button");
        button.GetAttribute("type").ShouldBe("submit");
        button.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-button hero-cta test-hook");
        var style = button.GetAttribute("style") ?? string.Empty;
        style.ShouldContain("--sc-fancy-ui-accent:#f00");
        style.ShouldContain("--sc-fancy-ui-focus-ring:#0ff");
        style.ShouldContain("margin:1rem");
        button.GetAttribute("aria-label").ShouldBe("Save changes");
    }

    [Fact]
    public void FancyButton_Disabled_SetsNativeDisabledAttribute()
    {
        using var context = CreateContext();

        var cut = context.Render<FancyButton>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Save"))
            .Add(component => component.Disabled, true));

        cut.Find("button").HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void FancyButton_Click_InvokesOnClick()
    {
        using var context = CreateContext();
        var clicked = false;

        var cut = context.Render<FancyButton>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Save"))
            .Add(component => component.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true)));

        cut.Find("button").Click();

        clicked.ShouldBeTrue();
    }

    [Fact]
    public void FancyButton_WithoutExplicitTheme_UsesConfiguredDefault()
    {
        var customTheme = new FancyUiTheme("#000", "#fff", "#222", "#abc", "2px", "0.5rem", "#def");
        using var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi(options => options.Theme = customTheme);

        var cut = context.Render<FancyButton>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddContent(0, "Save")));

        (cut.Find("button").GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-accent:#abc");
    }
}

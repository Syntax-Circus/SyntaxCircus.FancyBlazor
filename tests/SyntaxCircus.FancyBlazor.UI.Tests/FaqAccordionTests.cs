using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FaqAccordionTests
{
    private const string ModulePath = "./_content/SyntaxCircus.FancyBlazor.UI/js/faq-accordion.js";

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("create", _ => true);
        module.SetupVoid("destroy", _ => true);
        return context;
    }

    private static RenderFragment SingleItem => builder =>
    {
        builder.OpenComponent<FaqAccordionItem>(0);
        builder.AddAttribute(1, nameof(FaqAccordionItem.Question), (RenderFragment)(b => b.AddContent(0, "Question")));
        builder.AddAttribute(2, nameof(FaqAccordionItem.Answer), (RenderFragment)(b => b.AddContent(0, "Answer")));
        builder.CloseComponent();
    };

    [Fact]
    public void FaqAccordion_RendersRootWithStableHook()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordion>(parameters => parameters
            .Add(component => component.ChildContent, SingleItem));

        cut.Find("div").ClassList.ShouldContain("syntax-circus-fancy-ui-faq-accordion");
        cut.Find("button").ShouldNotBeNull();
    }

    [Fact]
    public void FaqAccordion_ImportsItsModuleOnFirstRender()
    {
        using var context = CreateContext();

        context.Render<FaqAccordion>(parameters => parameters
            .Add(component => component.ChildContent, SingleItem));

        context.JSInterop.VerifyInvoke("import");
    }

    [Fact]
    public void FaqAccordion_DefaultSingleOpenIsTrue()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordion>(parameters => parameters
            .Add(component => component.ChildContent, SingleItem));

        cut.Instance.SingleOpen.ShouldBeTrue();
    }

    [Fact]
    public void FaqAccordion_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<FaqAccordion>(parameters => parameters
            .Add(component => component.ChildContent, SingleItem)
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "marketing-faq")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var root = cut.Find("div");
        root.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-faq-accordion marketing-faq test-hook");
        (root.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-focus-ring:#0ff");
    }

    [Fact]
    public void FaqAccordion_RendersMultipleItemsWithDistinctIds()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordion>(parameters => parameters
            .Add(component => component.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<FaqAccordionItem>(0);
                builder.AddAttribute(1, nameof(FaqAccordionItem.Question), (RenderFragment)(b => b.AddContent(0, "Q1")));
                builder.AddAttribute(2, nameof(FaqAccordionItem.Answer), (RenderFragment)(b => b.AddContent(0, "A1")));
                builder.CloseComponent();
                builder.OpenComponent<FaqAccordionItem>(3);
                builder.AddAttribute(4, nameof(FaqAccordionItem.Question), (RenderFragment)(b => b.AddContent(0, "Q2")));
                builder.AddAttribute(5, nameof(FaqAccordionItem.Answer), (RenderFragment)(b => b.AddContent(0, "A2")));
                builder.CloseComponent();
            })));

        var buttons = cut.FindAll("button");
        buttons.Count.ShouldBe(2);
        buttons[0].GetAttribute("id").ShouldNotBe(buttons[1].GetAttribute("id"));
    }
}

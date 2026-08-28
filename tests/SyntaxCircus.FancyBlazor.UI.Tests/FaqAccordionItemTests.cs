using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class FaqAccordionItemTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    [Fact]
    public void FaqAccordionItem_DefaultsToClosed()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "What is FancyBlazor?"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "A Blazor effects library.")));

        var trigger = cut.Find("button");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find("[role='region']").HasAttribute("hidden").ShouldBeTrue();
    }

    [Fact]
    public void FaqAccordionItem_DefaultOpen_StartsExpanded()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "Question"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "Answer"))
            .Add(component => component.DefaultOpen, true));

        var trigger = cut.Find("button");
        trigger.GetAttribute("aria-expanded").ShouldBe("true");
        cut.Find("[role='region']").HasAttribute("hidden").ShouldBeFalse();
    }

    [Fact]
    public void FaqAccordionItem_TriggerAndPanel_ShareMatchingIds()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "Question"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "Answer")));

        var trigger = cut.Find("button");
        var panel = cut.Find("[role='region']");

        trigger.GetAttribute("aria-controls").ShouldBe(panel.GetAttribute("id"));
        panel.GetAttribute("aria-labelledby").ShouldBe(trigger.GetAttribute("id"));
    }

    [Fact]
    public void FaqAccordionItem_TwoInstances_GenerateDistinctIds()
    {
        using var context = CreateContext();

        var first = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "Q1"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "A1")));
        var second = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "Q2"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "A2")));

        first.Find("button").GetAttribute("id").ShouldNotBe(second.Find("button").GetAttribute("id"));
    }

    [Fact]
    public void FaqAccordionItem_RendersQuestionAndAnswerContent()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "What is FancyBlazor?"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "A Blazor effects library.")));

        cut.Find("button").TextContent.ShouldBe("What is FancyBlazor?");
        cut.Find("[role='region']").TextContent.ShouldBe("A Blazor effects library.");
    }

    [Fact]
    public void FaqAccordionItem_TriggerHasDataFaqTriggerMarker()
    {
        using var context = CreateContext();

        var cut = context.Render<FaqAccordionItem>(parameters => parameters
            .Add(component => component.Question, builder => builder.AddContent(0, "Question"))
            .Add(component => component.Answer, builder => builder.AddContent(0, "Answer")));

        cut.Find("button").HasAttribute("data-faq-trigger").ShouldBeTrue();
    }
}

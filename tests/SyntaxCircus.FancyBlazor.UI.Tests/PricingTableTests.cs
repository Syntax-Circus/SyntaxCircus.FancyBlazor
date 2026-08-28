using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.UI.Tests;

public sealed class PricingTableTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddFancyBlazorUi();
        return context;
    }

    private const string SamplePlans = """
        <thead>
            <tr><th scope="col">Feature</th><th scope="col">Free</th><th scope="col">Pro</th></tr>
        </thead>
        <tbody>
            <tr><th scope="row">Projects</th><td>3</td><td>Unlimited</td></tr>
        </tbody>
        """;

    [Fact]
    public void PricingTable_RendersTableWithStableHook()
    {
        using var context = CreateContext();

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans)));

        var table = cut.Find("table");
        table.ClassList.ShouldContain("syntax-circus-fancy-ui-pricing-table");
        cut.FindAll("th[scope='col']").Count.ShouldBe(3);
        cut.FindAll("th[scope='row']").Count.ShouldBe(1);
    }

    [Fact]
    public void PricingTable_WithoutAriaLabel_OmitsAttribute()
    {
        using var context = CreateContext();

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans)));

        cut.Find("table").HasAttribute("aria-label").ShouldBeFalse();
    }

    [Fact]
    public void PricingTable_WithAriaLabel_SetsAttribute()
    {
        using var context = CreateContext();

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.AriaLabel, "Plans")
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans)));

        cut.Find("table").GetAttribute("aria-label").ShouldBe("Plans");
    }

    [Fact]
    public void PricingTable_DefaultDensityIsComfortable()
    {
        using var context = CreateContext();

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans)));

        cut.Find("table").ClassList.ShouldNotContain("syntax-circus-fancy-ui-pricing-table--compact");
    }

    [Fact]
    public void PricingTable_CompactDensity_AddsModifierClass()
    {
        using var context = CreateContext();

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.Density, PricingTableDensity.Compact)
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans)));

        var table = cut.Find("table");
        table.ClassList.ShouldContain("syntax-circus-fancy-ui-pricing-table");
        table.ClassList.ShouldContain("syntax-circus-fancy-ui-pricing-table--compact");
    }

    [Fact]
    public void PricingTable_WithoutFeaturedTier_RendersWithoutAriaCurrent()
    {
        using var context = CreateContext();

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans)));

        cut.FindAll("[aria-current='true']").ShouldBeEmpty();
    }

    [Fact]
    public void PricingTable_MergesAttributesAndAppliesThemeTokens()
    {
        using var context = CreateContext();
        var theme = new FancyUiTheme("#111", "#eee", "#333", "#f00", "4px", "1rem", "#0ff");

        var cut = context.Render<PricingTable>(parameters => parameters
            .Add(component => component.ChildContent, builder => builder.AddMarkupContent(0, SamplePlans))
            .Add(component => component.Theme, theme)
            .Add(component => component.CssClass, "marketing-pricing")
            .Add(component => component.AdditionalAttributes, new Dictionary<string, object> { ["class"] = "test-hook" }));

        var table = cut.Find("table");
        table.GetAttribute("class").ShouldBe("syntax-circus-fancy-ui-pricing-table marketing-pricing test-hook");
        (table.GetAttribute("style") ?? string.Empty).ShouldContain("--sc-fancy-ui-surface:#111");
    }
}

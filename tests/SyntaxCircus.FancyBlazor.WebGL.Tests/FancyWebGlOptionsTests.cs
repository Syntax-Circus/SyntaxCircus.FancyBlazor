using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace SyntaxCircus.FancyBlazor.WebGL.Tests;

public sealed class FancyWebGlOptionsTests
{
    [Fact]
    public void AddFancyBlazorWebGl_WithoutConfiguration_UsesBoundedDefaultContextCapacity()
    {
        var services = new ServiceCollection();

        var returnedServices = services.AddFancyBlazorWebGl();

        returnedServices.ShouldBeSameAs(services);
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<FancyWebGlOptions>>().Value.MaxActiveContexts.ShouldBe(4);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(4, 4)]
    [InlineData(8, 8)]
    [InlineData(9, 8)]
    public void AddFancyBlazorWebGl_ClampsConfiguredContextCapacity(int configured, int expected)
    {
        var services = new ServiceCollection();

        services.AddFancyBlazorWebGl(options => options.MaxActiveContexts = configured);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<FancyWebGlOptions>>().Value.MaxActiveContexts.ShouldBe(expected);
    }
}

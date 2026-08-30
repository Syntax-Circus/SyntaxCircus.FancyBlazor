using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Tints and blurs semantic content in proportion to how fast the page is scrolling.</summary>
public partial class ScrollVelocity : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public FancyPalette Palette { get; set; } = FancyPalettes.Witchlight;
    [Parameter] public double Sensitivity { get; set; } = 1.5;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-scroll-velocity", CssClass,
        $"--sc-fancy-primary:{Palette.Primary};--sc-fancy-accent:{Palette.Accent}", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "scroll-velocity", ["data-fancy-disabled"] = Disabled ? "true" : "false" });

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { sensitivity = AttributeComposer.Clamp(Sensitivity, .1, 10) };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "scroll-velocity", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

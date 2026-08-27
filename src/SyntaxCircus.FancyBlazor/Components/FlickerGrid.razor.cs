using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Draws a bounded decorative grid of flickering cells behind semantic child content.</summary>
public partial class FlickerGrid : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public FancyPalette Palette { get; set; } = FancyPalettes.Witchlight;
    [Parameter] public int Density { get; set; } = 28;
    [Parameter] public double Speed { get; set; } = .35;
    [Parameter] public double Intensity { get; set; } = .5;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-flicker-grid", CssClass,
        $"--sc-fancy-primary:{Palette.Primary};--sc-fancy-secondary:{Palette.Secondary};--sc-fancy-accent:{Palette.Accent};--sc-fancy-background:{Palette.Background};--sc-fancy-flicker-grid-density:{Math.Clamp(Density, 1, 96)};--sc-fancy-flicker-grid-speed:{AttributeComposer.Number(AttributeComposer.Clamp(Speed, 0, 3))};--sc-fancy-flicker-grid-intensity:{AttributeComposer.Number(AttributeComposer.Clamp(Intensity, 0, 1))}", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "flicker-grid", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { palette = new[] { Palette.Primary, Palette.Secondary, Palette.Accent, Palette.Background }, density = Math.Clamp(Density, 1, 96), speed = AttributeComposer.Clamp(Speed, 0, 3), intensity = AttributeComposer.Clamp(Intensity, 0, 1) };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "flicker-grid", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }
    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

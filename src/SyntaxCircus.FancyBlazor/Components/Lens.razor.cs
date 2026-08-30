using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Shows a decorative pointer-following magnified view of a background image over semantic content.</summary>
public partial class Lens : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter, EditorRequired] public string ImageUrl { get; set; } = string.Empty;
    [Parameter] public double Zoom { get; set; } = 2.5;
    [Parameter] public double LensSize { get; set; } = 160;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-lens", CssClass,
        $"--sc-fancy-lens-image:url(\"{ImageUrl}\");--sc-fancy-lens-size:{AttributeComposer.Number(AttributeComposer.Clamp(LensSize, 60, 480))}px", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "lens", ["data-fancy-disabled"] = Disabled ? "true" : "false" });

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { zoom = AttributeComposer.Clamp(Zoom, 1.5, 5) };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "lens", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

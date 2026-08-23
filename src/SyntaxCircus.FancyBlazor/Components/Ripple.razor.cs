using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Adds a decorative ripple to pointer and tap interactions.</summary>
public partial class Ripple : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public string Color { get; set; } = "currentColor";
    [Parameter] public double Opacity { get; set; } = .24;
    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(600);
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-ripple", CssClass,
        $"--sc-fancy-ripple-color:{Color};--sc-fancy-ripple-opacity:{AttributeComposer.Number(AttributeComposer.Clamp(Opacity, 0, 1))};--sc-fancy-duration:{AttributeComposer.Number(AttributeComposer.NonNegative(Duration).TotalMilliseconds)}ms", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "ripple", ["data-fancy-disabled"] = Disabled ? "true" : "false" });

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { duration = AttributeComposer.NonNegative(Duration).TotalMilliseconds };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "ripple", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

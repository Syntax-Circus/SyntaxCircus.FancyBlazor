using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Draws a short-lived decorative trail behind pointer movement.</summary>
public partial class CursorTrail : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public string Color { get; set; } = "currentColor";
    [Parameter] public double Size { get; set; } = 16;
    [Parameter] public int ParticleCount { get; set; } = 16;
    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(450);
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-cursor-trail", CssClass,
        $"--sc-fancy-cursor-trail-color:{Color};--sc-fancy-cursor-trail-size:{AttributeComposer.Number(AttributeComposer.Clamp(Size, 4, 64))}px", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "cursor-trail", ["data-fancy-disabled"] = Disabled ? "true" : "false" });

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { color = Color, size = AttributeComposer.Clamp(Size, 4, 64), particleCount = Math.Clamp(ParticleCount, 1, 48), duration = AttributeComposer.NonNegative(Duration).TotalMilliseconds };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "cursor-trail", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

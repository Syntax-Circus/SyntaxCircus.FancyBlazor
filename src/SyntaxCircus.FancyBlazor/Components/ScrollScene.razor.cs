using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Continuously enhances a semantic section as it crosses the viewport.</summary>
public partial class ScrollScene : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public ScrollSceneEffect Effect { get; set; } = ScrollSceneEffect.Lift;
    [Parameter] public double Strength { get; set; } = .25;
    [Parameter] public double Travel { get; set; } = 48;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-scroll-scene", CssClass,
        $"--sc-fancy-scroll-scene-strength:{AttributeComposer.Number(AttributeComposer.Clamp(Strength, 0, 1))};--sc-fancy-scroll-scene-travel:{AttributeComposer.Number(AttributeComposer.Clamp(Travel, 0, 300))}px", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "scroll-scene", ["data-fancy-scroll-scene"] = Effect.ToString().ToLowerInvariant(), ["data-fancy-disabled"] = Disabled ? "true" : "false" });

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { effect = Effect.ToString(), strength = AttributeComposer.Clamp(Strength, 0, 1), travel = AttributeComposer.Clamp(Travel, 0, 300) };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "scroll-scene", options).ConfigureAwait(false); _signature = signature; }
        else if (!string.Equals(_signature, signature, StringComparison.Ordinal)) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

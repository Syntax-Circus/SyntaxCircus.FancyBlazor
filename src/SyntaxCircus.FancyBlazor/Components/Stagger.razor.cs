using System.Text.Json;
using Microsoft.AspNetCore.Components;
namespace SyntaxCircus.FancyBlazor;
/// <summary>Reveals direct element children in sequence when the wrapper enters the viewport.</summary>
public partial class Stagger : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public RevealEffect Effect { get; set; } = RevealEffect.FadeUp;
    [Parameter] public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(80);
    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(500);
    [Parameter] public double Distance { get; set; } = 16;
    [Parameter] public bool Once { get; set; } = true;
    /// <summary>Gets or sets a value that restarts the stagger sequence when it changes.</summary>
    [Parameter] public long ReplayToken { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-stagger", CssClass, $"--sc-fancy-stagger-delay:{AttributeComposer.Number(AttributeComposer.NonNegative(Delay).TotalMilliseconds)}ms;--sc-fancy-duration:{AttributeComposer.Number(AttributeComposer.NonNegative(Duration).TotalMilliseconds)}ms;--sc-fancy-distance:{AttributeComposer.Number(AttributeComposer.Clamp(Distance, 0, 500))}px", Style, AdditionalAttributes, new Dictionary<string, object> { ["data-fancy-effect"] = "stagger", ["data-fancy-reveal"] = Effect.ToString().ToLowerInvariant(), ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (Disabled) { await DestroyAsync(); return; } var options = new { effect = Effect.ToString(), delay = AttributeComposer.NonNegative(Delay).TotalMilliseconds, duration = AttributeComposer.NonNegative(Duration).TotalMilliseconds, distance = AttributeComposer.Clamp(Distance, 0, 500), once = Once, replayToken = ReplayToken }; var signature = JsonSerializer.Serialize(options); if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "stagger", options); _signature = signature; } else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options); _signature = signature; } }
    public async ValueTask DisposeAsync() { await DestroyAsync(); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle); }
}

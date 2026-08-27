using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Reveals semantic text through a character-scramble animation while keeping visual tokens decorative.</summary>
public partial class ScrambleText : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter, EditorRequired] public string Text { get; set; } = string.Empty;
    [Parameter] public TypeFlowElement Element { get; set; } = TypeFlowElement.Paragraph;
    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(600);
    [Parameter] public TimeSpan Stagger { get; set; } = TimeSpan.FromMilliseconds(24);
    [Parameter] public bool Once { get; set; } = true;
    [Parameter] public long ReplayToken { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-scramble-text", CssClass,
        null, Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "scramble-text", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { text = Text, duration = AttributeComposer.NonNegative(Duration).TotalMilliseconds, stagger = AttributeComposer.NonNegative(Stagger).TotalMilliseconds, once = Once, replayToken = ReplayToken };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "scramble-text", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }
    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

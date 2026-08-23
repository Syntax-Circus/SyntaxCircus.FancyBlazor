using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Reveals plain text by word or character while retaining semantic markup.</summary>
public partial class TextReveal : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter, EditorRequired] public string Text { get; set; } = string.Empty;
    [Parameter] public TextRevealElement Element { get; set; } = TextRevealElement.Span;
    [Parameter] public TextRevealUnit Unit { get; set; } = TextRevealUnit.Word;
    [Parameter] public RevealEffect Effect { get; set; } = RevealEffect.FadeUp;
    [Parameter] public TimeSpan Delay { get; set; }
    [Parameter] public TimeSpan Stagger { get; set; } = TimeSpan.FromMilliseconds(70);
    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(500);
    [Parameter] public double Distance { get; set; } = 16;
    [Parameter] public bool Once { get; set; } = true;
    [Parameter] public long ReplayToken { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-text-reveal", CssClass,
        $"--sc-fancy-delay:{AttributeComposer.Number(AttributeComposer.NonNegative(Delay).TotalMilliseconds)}ms;--sc-fancy-stagger:{AttributeComposer.Number(AttributeComposer.NonNegative(Stagger).TotalMilliseconds)}ms;--sc-fancy-duration:{AttributeComposer.Number(AttributeComposer.NonNegative(Duration).TotalMilliseconds)}ms;--sc-fancy-distance:{AttributeComposer.Number(AttributeComposer.Clamp(Distance, 0, 500))}px", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "text-reveal", ["data-fancy-reveal"] = Effect.ToString().ToLowerInvariant(), ["data-fancy-disabled"] = Disabled ? "true" : "false" });

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { text = Text, unit = Unit.ToString(), effect = Effect.ToString(), once = Once, replayToken = ReplayToken };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "text-reveal", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

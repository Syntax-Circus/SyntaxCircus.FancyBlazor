using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Reveals one of two pieces of content against the other by dragging a handle.</summary>
public partial class CompareReveal : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public RenderFragment? Before { get; set; }
    [Parameter, EditorRequired] public RenderFragment? After { get; set; }
    [Parameter] public CompareRevealOrientation Orientation { get; set; } = CompareRevealOrientation.Horizontal;
    [Parameter] public double InitialPosition { get; set; } = 50;
    [Parameter] public string? BeforeLabel { get; set; }
    [Parameter] public string? AfterLabel { get; set; }
    [Parameter] public IReadOnlyList<double>? SnapPoints { get; set; }
    [Parameter] public string AriaLabel { get; set; } = "Comparison position";

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private double ClampedInitialPosition => AttributeComposer.Clamp(InitialPosition, 0, 100);

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-compare-reveal", CssClass,
        $"--sc-fancy-compare-reveal-position:{AttributeComposer.Number(ClampedInitialPosition)}%", Style, AdditionalAttributes,
        new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "compare-reveal",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-compare-reveal-orientation"] = Orientation.ToString().ToLowerInvariant(),
        });

    protected override void OnInitialized()
    {
        if (Before is null || After is null)
        {
            throw new InvalidOperationException("CompareReveal requires both Before and After content.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new
        {
            orientation = Orientation.ToString().ToLowerInvariant(),
            snapPoints = SnapPoints?.Select(point => AttributeComposer.Clamp(point, 0, 100)).ToArray(),
        };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "compare-reveal", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }

    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

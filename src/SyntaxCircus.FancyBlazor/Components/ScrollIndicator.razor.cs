using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Adds a decorative local reading-progress line around semantic content.</summary>
public partial class ScrollIndicator : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public string Color { get; set; } = "currentColor";
    [Parameter] public double Thickness { get; set; } = 2;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-scroll-indicator", CssClass,
        $"--sc-fancy-scroll-indicator-color:{Color};--sc-fancy-scroll-indicator-thickness:{AttributeComposer.Number(AttributeComposer.Clamp(Thickness, 1, 12))}px", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "scroll-indicator", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; } if (_handle is null) _handle = await Runtime.CreateAsync(_element, "scroll-indicator", new { }).ConfigureAwait(false); }
    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

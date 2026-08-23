using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Adds a palette-derived decorative backdrop that responds to local scroll progress.</summary>
public partial class ScrollBackdrop : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public FancyPalette Palette { get; set; } = FancyPalettes.Witchlight;
    [Parameter] public double Intensity { get; set; } = .25;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-scroll-backdrop", CssClass,
        $"--sc-fancy-primary:{Palette.Primary};--sc-fancy-secondary:{Palette.Secondary};--sc-fancy-accent:{Palette.Accent};--sc-fancy-background:{Palette.Background};--sc-fancy-scroll-backdrop-intensity:{AttributeComposer.Number(AttributeComposer.Clamp(Intensity, 0, 1))}", Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "scroll-backdrop", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; } if (_handle is null) _handle = await Runtime.CreateAsync(_element, "scroll-backdrop", new { }).ConfigureAwait(false); }
    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
}

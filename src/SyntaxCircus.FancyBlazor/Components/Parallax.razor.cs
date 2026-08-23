using Microsoft.AspNetCore.Components;
namespace SyntaxCircus.FancyBlazor;
/// <summary>Offsets semantic content subtly as it moves through the viewport.</summary>
public partial class Parallax : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public double Distance { get; set; } = 24;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-parallax", CssClass, $"--sc-fancy-parallax-distance:{AttributeComposer.Number(AttributeComposer.Clamp(Distance, 0, 300))}px", Style, AdditionalAttributes, new Dictionary<string, object> { ["data-fancy-effect"] = "parallax", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (Disabled) { await DestroyAsync(); return; } if (_handle is null) _handle = await Runtime.CreateAsync(_element, "parallax", new { }); }
    public async ValueTask DisposeAsync() { await DestroyAsync(); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; await Runtime.DestroyAsync(handle); }
}

using System.Text.Json;
using Microsoft.AspNetCore.Components;
namespace SyntaxCircus.FancyBlazor;
/// <summary>Adds a decorative pointer-following light behind semantic content.</summary>
public partial class Spotlight : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public string Color { get; set; } = "currentColor";
    [Parameter] public double Size { get; set; } = 320;
    [Parameter] public double Opacity { get; set; } = .25;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-spotlight", CssClass, $"--sc-fancy-spotlight-color:{Color};--sc-fancy-spotlight-size:{AttributeComposer.Number(AttributeComposer.Clamp(Size, 32, 1200))}px;--sc-fancy-spotlight-opacity:{AttributeComposer.Number(AttributeComposer.Clamp(Opacity, 0, 1))}", Style, AdditionalAttributes, new Dictionary<string, object> { ["data-fancy-effect"] = "spotlight", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (Disabled) { await DestroyAsync(); return; } var options = new { }; var signature = JsonSerializer.Serialize(options); if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "spotlight", options); _signature = signature; } }
    public async ValueTask DisposeAsync() { await DestroyAsync(); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle); }
}

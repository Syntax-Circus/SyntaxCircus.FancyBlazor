using System.Text.Json;
using Microsoft.AspNetCore.Components;
namespace SyntaxCircus.FancyBlazor;
/// <summary>Applies a subtle pointer-relative transform without changing child interaction.</summary>
public partial class Magnetic : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public double Strength { get; set; } = .2;
    [Parameter] public TimeSpan ResetDuration { get; set; } = TimeSpan.FromMilliseconds(250);
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter, EditorRequired] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-magnetic", CssClass, $"--sc-fancy-magnetic-strength:{AttributeComposer.Number(AttributeComposer.Clamp(Strength, 0, 1))};--sc-fancy-reset-duration:{AttributeComposer.Number(AttributeComposer.NonNegative(ResetDuration).TotalMilliseconds)}ms", Style, AdditionalAttributes, new Dictionary<string, object> { ["data-fancy-effect"] = "magnetic", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (Disabled) { await DestroyAsync(); return; } var options = new { strength = AttributeComposer.Clamp(Strength, 0, 1) }; if (_handle is null) _handle = await Runtime.CreateAsync(_element, "magnetic", options); }
    public async ValueTask DisposeAsync() { await DestroyAsync(); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; await Runtime.DestroyAsync(handle); }
}

using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Animates a numeric display toward a target value while an always-correct value stays accessible.</summary>
public partial class NumberTicker : ComponentBase, IAsyncDisposable
{
    private ElementReference _element; private long? _handle; private string? _signature;
    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;
    [Parameter] public double Value { get; set; }
    [Parameter] public string? Format { get; set; }
    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(1200);
    [Parameter] public bool Once { get; set; } = true;
    [Parameter] public long ReplayToken { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private string FormattedValue => Value.ToString(string.IsNullOrEmpty(Format) ? "0" : Format, CultureInfo.InvariantCulture);
    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose("syntax-circus-fancy-number-ticker", CssClass,
        null, Style, AdditionalAttributes,
        new Dictionary<string, object> { ["data-fancy-effect"] = "number-ticker", ["data-fancy-disabled"] = Disabled ? "true" : "false" });
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled) { await DestroyAsync().ConfigureAwait(false); return; }
        var options = new { value = Value, decimals = DeriveDecimals(Format), formatted = FormattedValue, duration = AttributeComposer.NonNegative(Duration).TotalMilliseconds, once = Once, replayToken = ReplayToken };
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null) { _handle = await Runtime.CreateAsync(_element, "number-ticker", options).ConfigureAwait(false); _signature = signature; }
        else if (_signature != signature) { await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false); _signature = signature; }
    }
    public async ValueTask DisposeAsync() { await DestroyAsync().ConfigureAwait(false); GC.SuppressFinalize(this); }
    private async ValueTask DestroyAsync() { if (_handle is not { } handle) return; _handle = null; _signature = null; await Runtime.DestroyAsync(handle).ConfigureAwait(false); }
    private static int DeriveDecimals(string? format)
    {
        if (string.IsNullOrEmpty(format)) return 0;
        if (char.IsLetter(format[0]))
        {
            var digits = format[1..];
            return digits.Length > 0 && int.TryParse(digits, out var precision) ? precision : 0;
        }
        var dot = format.IndexOf('.');
        if (dot < 0) return 0;
        var count = 0;
        for (var i = dot + 1; i < format.Length && (format[i] == '0' || format[i] == '#'); i++) count++;
        return count;
    }
}

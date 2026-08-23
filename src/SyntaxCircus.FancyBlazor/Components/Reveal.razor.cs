using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Reveals semantic content when it enters the viewport.</summary>
public partial class Reveal : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject]
    private IFancyEffectRuntime Runtime { get; set; } = default!;

    /// <summary>Gets or sets the reveal transition.</summary>
    [Parameter]
    public RevealEffect Effect { get; set; } = RevealEffect.FadeUp;

    /// <summary>Gets or sets the delay before the transition starts.</summary>
    [Parameter]
    public TimeSpan Delay { get; set; }

    /// <summary>Gets or sets transition duration.</summary>
    [Parameter]
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Gets or sets travel distance in CSS pixels.</summary>
    [Parameter]
    public double Distance { get; set; } = 16;

    /// <summary>Gets or sets whether the reveal remains visible after its first entry.</summary>
    [Parameter]
    public bool Once { get; set; } = true;

    /// <summary>Gets or sets whether observer behavior is disabled.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Gets or sets an additional class for the outer wrapper.</summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>Gets or sets additional inline styles for the outer wrapper.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Gets or sets content to reveal.</summary>
    [Parameter, EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets unmatched attributes applied to the outer wrapper.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-reveal",
        CssClass,
        BuildStyle(),
        Style,
        AdditionalAttributes,
        new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "reveal",
            ["data-fancy-reveal"] = Effect.ToString().ToLowerInvariant(),
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
        });

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var options = CreateOptions();
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "reveal", options).ConfigureAwait(false);
            _signature = signature;
        }
        else if (!string.Equals(_signature, signature, StringComparison.Ordinal))
        {
            await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false);
            _signature = signature;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DestroyAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private object CreateOptions() => new
    {
        effect = Effect.ToString(),
        delay = AttributeComposer.NonNegative(Delay).TotalMilliseconds,
        duration = AttributeComposer.NonNegative(Duration).TotalMilliseconds,
        distance = AttributeComposer.Clamp(Distance, 0, 500),
        once = Once,
    };

    private string BuildStyle() =>
        $"--sc-fancy-delay:{AttributeComposer.Number(AttributeComposer.NonNegative(Delay).TotalMilliseconds)}ms;--sc-fancy-duration:{AttributeComposer.Number(AttributeComposer.NonNegative(Duration).TotalMilliseconds)}ms;--sc-fancy-distance:{AttributeComposer.Number(AttributeComposer.Clamp(Distance, 0, 500))}px";

    private async ValueTask DestroyAsync()
    {
        if (_handle is not { } handle)
        {
            return;
        }

        _handle = null;
        _signature = null;
        await Runtime.DestroyAsync(handle).ConfigureAwait(false);
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Crossfades or character-splits between a list of strings while holding each for a visible beat.</summary>
public partial class MorphText : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<string> Words { get; set; } = Array.Empty<string>();

    [Parameter] public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(600);
    [Parameter] public TimeSpan Hold { get; set; } = TimeSpan.FromSeconds(1.2);
    [Parameter] public bool Loop { get; set; } = true;
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public MorphMode Mode { get; set; } = MorphMode.Crossfade;
    [Parameter] public string? Easing { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-morph-text", CssClass,
        null, Style, AdditionalAttributes,
        BuildFixedAttributes());

    private Dictionary<string, object> BuildFixedAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "morph-text",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-morph-mode"] = Mode == MorphMode.CharSplit ? "char-split" : "crossfade",
        };
        if (Disabled)
        {
            attrs["class"] = "syntax-circus-fancy-kinetic-text--static";
        }
        return attrs;
    }

    protected override void OnInitialized()
    {
        if (Words is null || Words.Count < 2)
        {
            throw new InvalidOperationException("MorphText requires at least two words.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var durationMs = Math.Clamp(AttributeComposer.NonNegative(Duration).TotalMilliseconds, 100, 2000);
        var holdMs = Math.Clamp(AttributeComposer.NonNegative(Hold).TotalMilliseconds, 0, 10000);
        var options = new
        {
            words = Words,
            duration = durationMs,
            hold = holdMs,
            loop = Loop,
            startIndex = Math.Max(0, StartIndex),
            mode = Mode == MorphMode.CharSplit ? "char-split" : "crossfade",
            easing = Easing,
        };
        var signature = JsonSerializer.Serialize(options);

        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "morph-text", options).ConfigureAwait(false);
            _signature = signature;
        }
        else if (_signature != signature)
        {
            await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false);
            _signature = signature;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DestroyAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DestroyAsync()
    {
        if (_handle is not { } handle) return;
        _handle = null;
        _signature = null;
        await Runtime.DestroyAsync(handle).ConfigureAwait(false);
    }
}

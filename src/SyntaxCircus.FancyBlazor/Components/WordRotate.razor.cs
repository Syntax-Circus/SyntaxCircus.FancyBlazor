using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Cycles through a list of headline words with a transition between each word while keeping the visible text decorative.</summary>
public partial class WordRotate : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<string> Words { get; set; } = Array.Empty<string>();

    [Parameter] public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(2.5);
    [Parameter] public bool Loop { get; set; } = true;
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public WordRotateTransition Transition { get; set; } = WordRotateTransition.Fade;
    [Parameter] public string? Easing { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CurrentWord => Words.Count == 0 ? string.Empty : Words[Math.Clamp(StartIndex, 0, Words.Count - 1)];

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        Disabled ? "syntax-circus-fancy-word-rotate syntax-circus-fancy-kinetic-text--static" : "syntax-circus-fancy-word-rotate",
        CssClass,
        null, Style, AdditionalAttributes,
        BuildFixedAttributes());

    private Dictionary<string, object> BuildFixedAttributes()
    {
        return new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "word-rotate",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-word-rotate-transition"] = Transition switch
            {
                WordRotateTransition.Fade => "fade",
                WordRotateTransition.SlideUp => "slide-up",
                WordRotateTransition.SlideDown => "slide-down",
                WordRotateTransition.Blur => "blur",
                _ => "fade",
            },
        };
    }

    protected override void OnInitialized()
    {
        if (Words is null || Words.Count < 2)
        {
            throw new InvalidOperationException("WordRotate requires at least two words.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var intervalMs = Math.Clamp(AttributeComposer.NonNegative(Interval).TotalMilliseconds, 250, 30000);
        var options = new
        {
            words = Words,
            interval = intervalMs,
            loop = Loop,
            startIndex = Math.Max(0, StartIndex),
            transition = Transition.ToString().ToLowerInvariant(),
            easing = Easing,
        };
        var signature = JsonSerializer.Serialize(options);

        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "word-rotate", options).ConfigureAwait(false);
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

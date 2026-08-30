using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Progressively types a list of lines character by character with an optional blinking caret.</summary>
public partial class Typewriter : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject] private IFancyEffectRuntime Runtime { get; set; } = default!;

    [Parameter, EditorRequired] public IReadOnlyList<string> Text { get; set; } = Array.Empty<string>();

    [Parameter] public TimeSpan Speed { get; set; } = TimeSpan.FromMilliseconds(60);
    [Parameter] public TimeSpan HoldAfter { get; set; } = TimeSpan.FromSeconds(1.5);
    [Parameter] public TimeSpan? DeleteSpeed { get; set; }
    [Parameter] public bool Loop { get; set; } = true;
    [Parameter] public int StartIndex { get; set; }
    [Parameter] public bool Caret { get; set; } = true;
    [Parameter] public string CaretCharacter { get; set; } = "|";
    [Parameter] public KineticTextDirection Direction { get; set; } = KineticTextDirection.Auto;

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? CssClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-typewriter", CssClass,
        null, Style, AdditionalAttributes,
        BuildFixedAttributes());

    private Dictionary<string, object> BuildFixedAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "typewriter",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
            ["data-fancy-typewriter-caret"] = Caret ? "true" : "false",
            ["data-fancy-typewriter-direction"] = Direction.ToString().ToLowerInvariant(),
        };
        if (Disabled)
        {
            attrs["class"] = "syntax-circus-fancy-kinetic-text--static";
        }
        return attrs;
    }

    protected override void OnInitialized()
    {
        if (Text is null || Text.Count == 0)
        {
            throw new InvalidOperationException("Typewriter requires at least one line.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Disabled)
        {
            await DestroyAsync().ConfigureAwait(false);
            return;
        }

        var speedMs = Math.Clamp(AttributeComposer.NonNegative(Speed).TotalMilliseconds, 10, 500);
        var holdAfterMs = Math.Clamp(AttributeComposer.NonNegative(HoldAfter).TotalMilliseconds, 0, 30000);
        var options = new
        {
            text = Text,
            speed = speedMs,
            holdAfter = holdAfterMs,
            deleteSpeed = DeleteSpeed is null ? (double?)null : Math.Clamp(AttributeComposer.NonNegative(DeleteSpeed.Value).TotalMilliseconds, 10, 500),
            loop = Loop,
            startIndex = Math.Max(0, StartIndex),
            caret = Caret,
            caretCharacter = CaretCharacter,
        };
        var signature = JsonSerializer.Serialize(options);

        if (_handle is null)
        {
            _handle = await Runtime.CreateAsync(_element, "typewriter", options).ConfigureAwait(false);
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

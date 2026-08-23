using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Renders a progressively enhanced decorative shader behind semantic child content.</summary>
public partial class ShaderBackground : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject]
    private IFancyEffectRuntime Runtime { get; set; } = default!;

    /// <summary>Gets or sets the built-in shader.</summary>
    [Parameter]
    public ShaderEffect Effect { get; set; } = ShaderEffect.Nacre;

    /// <summary>Gets or sets the four-color effect palette.</summary>
    [Parameter]
    public FancyPalette Palette { get; set; } = FancyPalettes.Witchlight;

    /// <summary>Gets or sets the animation speed multiplier.</summary>
    [Parameter]
    public double Speed { get; set; } = 1;

    /// <summary>Gets or sets visual intensity from zero through one.</summary>
    [Parameter]
    public double Intensity { get; set; } = 0.5;

    /// <summary>Gets or sets whether pointer position influences the shader.</summary>
    [Parameter]
    public bool Interactive { get; set; }

    /// <summary>Gets or sets a per-component quality override.</summary>
    [Parameter]
    public FancyQuality? Quality { get; set; }

    /// <summary>Gets or sets whether the effect runtime is disabled.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Gets or sets an additional class for the outer wrapper.</summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>Gets or sets additional inline styles for the outer wrapper.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Gets or sets semantic content rendered above the decorative layer.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets unmatched attributes applied to the outer wrapper.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-shader-background",
        CssClass,
        BuildStyle(),
        Style,
        AdditionalAttributes,
        new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "shader-background",
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
            _handle = await Runtime.CreateAsync(_element, "shader-background", options).ConfigureAwait(false);
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
        palette = new[] { Palette.Primary, Palette.Secondary, Palette.Accent, Palette.Background },
        speed = AttributeComposer.Clamp(Speed, 0, 3),
        intensity = AttributeComposer.Clamp(Intensity, 0, 1),
        interactive = Interactive,
        quality = Quality?.ToString(),
    };

    private string BuildStyle() =>
        $"--sc-fancy-primary:{Palette.Primary};--sc-fancy-secondary:{Palette.Secondary};--sc-fancy-accent:{Palette.Accent};--sc-fancy-background:{Palette.Background}";

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

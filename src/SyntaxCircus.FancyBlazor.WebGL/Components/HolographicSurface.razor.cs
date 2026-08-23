using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Renders a progressively enhanced holographic surface behind semantic child content.</summary>
public partial class HolographicSurface : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject]
    private IFancyWebGlRuntime Runtime { get; set; } = default!;

    /// <summary>Gets or sets the four-color surface palette.</summary>
    [Parameter] public FancyPalette Palette { get; set; } = FancyPalettes.Witchlight;

    /// <summary>Gets or sets visual intensity from zero through one.</summary>
    [Parameter] public double Intensity { get; set; } = 0.5;

    /// <summary>Gets or sets the apparent surface depth from zero through one.</summary>
    [Parameter] public double Depth { get; set; } = 0.5;

    /// <summary>Gets or sets the highlight sheen from zero through one.</summary>
    [Parameter] public double Sheen { get; set; } = 0.5;

    /// <summary>Gets or sets the animation speed multiplier.</summary>
    [Parameter] public double Speed { get; set; } = 1;

    /// <summary>Gets or sets whether fine-pointer position influences the surface.</summary>
    [Parameter] public bool Interactive { get; set; }

    /// <summary>Gets or sets a per-component quality override.</summary>
    [Parameter] public FancyQuality? Quality { get; set; }

    /// <summary>Gets or sets whether the optional WebGL runtime is disabled.</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Gets or sets an additional class for the outer wrapper.</summary>
    [Parameter] public string? CssClass { get; set; }

    /// <summary>Gets or sets additional inline styles for the outer wrapper.</summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>Gets or sets semantic content rendered above the decorative layer.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets unmatched attributes applied to the outer wrapper.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => WebGlAttributeComposer.Compose(
        "syntax-circus-fancy-holographic-surface",
        CssClass,
        BuildStyle(),
        Style,
        AdditionalAttributes,
        new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "holographic-surface",
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
            _handle = await Runtime.CreateAsync(_element, "holographic-surface", options).ConfigureAwait(false);
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
        palette = new[] { Palette.Primary, Palette.Secondary, Palette.Accent, Palette.Background },
        intensity = WebGlAttributeComposer.Clamp(Intensity, 0, 1),
        depth = WebGlAttributeComposer.Clamp(Depth, 0, 1),
        sheen = WebGlAttributeComposer.Clamp(Sheen, 0, 1),
        speed = WebGlAttributeComposer.Clamp(Speed, 0, 3),
        interactive = Interactive,
        quality = Quality?.ToString(),
    };

    private string BuildStyle() =>
        $"--sc-fancy-primary:{Palette.Primary};--sc-fancy-secondary:{Palette.Secondary};--sc-fancy-accent:{Palette.Accent};--sc-fancy-background:{Palette.Background};--sc-fancy-holographic-intensity:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Intensity, 0, 1))};--sc-fancy-holographic-depth:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Depth, 0, 1))};--sc-fancy-holographic-sheen:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Sheen, 0, 1))};--sc-fancy-holographic-speed:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Speed, 0, 3))}";

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

using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Renders a progressively enhanced interference wave field behind semantic child content.</summary>
public partial class WaveFieldBackground : ComponentBase, IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private ElementReference _element;
    private long? _handle;
    private string? _signature;
    private long _canvasGeneration;
    private volatile bool _disposed;

    [Inject]
    private IFancyWebGlRuntime Runtime { get; set; } = default!;

    /// <summary>Gets or sets the four-color field palette.</summary>
    [Parameter] public FancyPalette Palette { get; set; } = FancyPalettes.Witchlight;

    /// <summary>Gets or sets visual intensity from zero through one.</summary>
    [Parameter] public double Intensity { get; set; } = 0.5;

    /// <summary>Gets or sets wave displacement height from zero through one.</summary>
    [Parameter] public double Amplitude { get; set; } = 0.5;

    /// <summary>Gets or sets wave tightness/count from zero through one.</summary>
    [Parameter] public double Frequency { get; set; } = 0.5;

    /// <summary>Gets or sets the bright crest highlight strength from zero through one.</summary>
    [Parameter] public double Foam { get; set; } = 0.5;

    /// <summary>Gets or sets the animation speed multiplier.</summary>
    [Parameter] public double Speed { get; set; } = 1;

    /// <summary>Gets or sets whether fine-pointer position influences the wave field.</summary>
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
        "syntax-circus-fancy-wave-field-background",
        CssClass,
        BuildStyle(),
        Style,
        AdditionalAttributes,
        new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "wave-field-background",
            ["data-fancy-disabled"] = Disabled ? "true" : "false",
        });

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var replaceCanvas = false;
        await _lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
            {
                return;
            }

            replaceCanvas = Disabled
                ? await DestroyActiveHandleAsync(replaceCanvas: true).ConfigureAwait(false)
                : await CreateOrUpdateAsync().ConfigureAwait(false);
        }
        finally
        {
            _lifecycleGate.Release();
        }

        if (replaceCanvas)
        {
            await InvokeAsync(() =>
            {
                if (!_disposed)
                {
                    StateHasChanged();
                }
            });
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        await _lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            await DestroyActiveHandleAsync(replaceCanvas: false).ConfigureAwait(false);
        }
        finally
        {
            _lifecycleGate.Release();
        }

        GC.SuppressFinalize(this);
    }

    private object CreateOptions() => new
    {
        palette = new[] { Palette.Primary, Palette.Secondary, Palette.Accent, Palette.Background },
        intensity = WebGlAttributeComposer.Clamp(Intensity, 0, 1),
        amplitude = WebGlAttributeComposer.Clamp(Amplitude, 0, 1),
        frequency = WebGlAttributeComposer.Clamp(Frequency, 0, 1),
        foam = WebGlAttributeComposer.Clamp(Foam, 0, 1),
        speed = WebGlAttributeComposer.Clamp(Speed, 0, 3),
        interactive = Interactive,
        quality = Quality?.ToString(),
    };

    private string BuildStyle() =>
        $"--sc-fancy-primary:{Palette.Primary};--sc-fancy-secondary:{Palette.Secondary};--sc-fancy-accent:{Palette.Accent};--sc-fancy-background:{Palette.Background};--sc-fancy-wave-field-intensity:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Intensity, 0, 1))};--sc-fancy-wave-field-amplitude:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Amplitude, 0, 1))};--sc-fancy-wave-field-frequency:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Frequency, 0, 1))};--sc-fancy-wave-field-foam:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Foam, 0, 1))};--sc-fancy-wave-field-speed:{WebGlAttributeComposer.Number(WebGlAttributeComposer.Clamp(Speed, 0, 3))}";

    private async ValueTask<bool> CreateOrUpdateAsync()
    {
        var options = CreateOptions();
        var signature = JsonSerializer.Serialize(options);
        if (_handle is null)
        {
            var createdHandle = await Runtime.CreateAsync(_element, "wave-field-background", options).ConfigureAwait(false);
            if (createdHandle is not { } handle)
            {
                return false;
            }

            if (_disposed || Disabled)
            {
                var replaceCanvas = !_disposed;
                if (replaceCanvas)
                {
                    Interlocked.Increment(ref _canvasGeneration);
                }

                await Runtime.DestroyAsync(handle).ConfigureAwait(false);
                return replaceCanvas;
            }

            _handle = handle;
            _signature = signature;
        }
        else if (!string.Equals(_signature, signature, StringComparison.Ordinal))
        {
            await Runtime.UpdateAsync(_handle.Value, options).ConfigureAwait(false);
            _signature = signature;
        }

        return false;
    }

    private async ValueTask<bool> DestroyActiveHandleAsync(bool replaceCanvas)
    {
        if (_handle is not { } handle)
        {
            return false;
        }

        _handle = null;
        _signature = null;
        if (replaceCanvas)
        {
            Interlocked.Increment(ref _canvasGeneration);
        }

        await Runtime.DestroyAsync(handle).ConfigureAwait(false);
        return replaceCanvas;
    }
}

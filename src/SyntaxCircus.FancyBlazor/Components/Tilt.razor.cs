using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace SyntaxCircus.FancyBlazor;

/// <summary>Adds pointer-driven perspective motion around semantic child content.</summary>
public partial class Tilt : ComponentBase, IAsyncDisposable
{
    private ElementReference _element;
    private long? _handle;
    private string? _signature;

    [Inject]
    private IFancyEffectRuntime Runtime { get; set; } = default!;

    /// <summary>Gets or sets the maximum rotation in degrees.</summary>
    [Parameter]
    public double MaxAngle { get; set; } = 10;

    /// <summary>Gets or sets perspective depth in CSS pixels.</summary>
    [Parameter]
    public double Perspective { get; set; } = 800;

    /// <summary>Gets or sets the scale applied at maximum engagement.</summary>
    [Parameter]
    public double Scale { get; set; } = 1;

    /// <summary>Gets or sets whether a decorative glare layer is rendered.</summary>
    [Parameter]
    public bool Glare { get; set; }

    /// <summary>Gets or sets glare opacity from zero through one.</summary>
    [Parameter]
    public double GlareOpacity { get; set; } = 0.2;

    /// <summary>Gets or sets the pointer-leave reset duration.</summary>
    [Parameter]
    public TimeSpan ResetDuration { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Gets or sets whether pointer motion is disabled.</summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Gets or sets an additional class for the outer wrapper.</summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>Gets or sets additional inline styles for the outer wrapper.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Gets or sets content receiving the perspective effect.</summary>
    [Parameter, EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets unmatched attributes applied to the outer wrapper.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyDictionary<string, object> RootAttributes => AttributeComposer.Compose(
        "syntax-circus-fancy-tilt",
        CssClass,
        BuildStyle(),
        Style,
        AdditionalAttributes,
        new Dictionary<string, object>
        {
            ["data-fancy-effect"] = "tilt",
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
            _handle = await Runtime.CreateAsync(_element, "tilt", options).ConfigureAwait(false);
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
        maxAngle = AttributeComposer.Clamp(MaxAngle, 0, 45),
        perspective = AttributeComposer.Clamp(Perspective, 100, 4000),
        scale = AttributeComposer.Clamp(Scale, 0.8, 1.25),
        glare = Glare,
        glareOpacity = AttributeComposer.Clamp(GlareOpacity, 0, 1),
        resetDuration = AttributeComposer.NonNegative(ResetDuration).TotalMilliseconds,
    };

    private string BuildStyle() =>
        $"--sc-fancy-perspective:{AttributeComposer.Number(AttributeComposer.Clamp(Perspective, 100, 4000))}px;--sc-fancy-scale:{AttributeComposer.Number(AttributeComposer.Clamp(Scale, 0.8, 1.25))};--sc-fancy-glare-opacity:{AttributeComposer.Number(AttributeComposer.Clamp(GlareOpacity, 0, 1))};--sc-fancy-reset-duration:{AttributeComposer.Number(AttributeComposer.NonNegative(ResetDuration).TotalMilliseconds)}ms";

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

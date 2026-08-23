namespace SyntaxCircus.FancyBlazor;

/// <summary>Global defaults applied to FancyBlazor effects.</summary>
public sealed class FancyBlazorOptions
{
    /// <summary>Gets or sets the motion preference. Defaults to the browser setting.</summary>
    public FancyMotionPreference MotionPreference { get; set; } = FancyMotionPreference.RespectSystem;

    /// <summary>Gets or sets the default rendering quality.</summary>
    public FancyQuality Quality { get; set; } = FancyQuality.Auto;

    /// <summary>Gets or sets whether continuous effects stop in hidden documents.</summary>
    public bool PauseWhenHidden { get; set; } = true;

    /// <summary>Gets or sets whether continuous effects stop while offscreen.</summary>
    public bool PauseWhenOffscreen { get; set; } = true;

    /// <summary>Gets or sets whether unsupported diagnostic hooks are enabled.</summary>
    public bool EnableDiagnostics { get; set; }
}

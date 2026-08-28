namespace SyntaxCircus.FancyBlazor;

/// <summary>Global defaults applied to FancyBlazor UI companion controls.</summary>
public sealed class FancyUiOptions
{
    /// <summary>Gets or sets the default theme applied when a control's <c>Theme</c> parameter is not set.</summary>
    public FancyUiTheme Theme { get; set; } = FancyUiThemes.Default;
}

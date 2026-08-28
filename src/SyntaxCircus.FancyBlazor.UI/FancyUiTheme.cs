namespace SyntaxCircus.FancyBlazor;

/// <summary>Typed styling tokens shared by FancyBlazor UI companion controls.</summary>
public sealed record FancyUiTheme
{
    /// <summary>Creates a theme from CSS token values.</summary>
    public FancyUiTheme(
        string surface,
        string text,
        string border,
        string accent,
        string radius,
        string spacing,
        string focusRing)
    {
        Surface = RequireValue(surface, nameof(surface));
        Text = RequireValue(text, nameof(text));
        Border = RequireValue(border, nameof(border));
        Accent = RequireValue(accent, nameof(accent));
        Radius = RequireValue(radius, nameof(radius));
        Spacing = RequireValue(spacing, nameof(spacing));
        FocusRing = RequireValue(focusRing, nameof(focusRing));
    }

    /// <summary>Gets the surface (background) CSS color.</summary>
    public string Surface { get; }

    /// <summary>Gets the primary text CSS color.</summary>
    public string Text { get; }

    /// <summary>Gets the border CSS color.</summary>
    public string Border { get; }

    /// <summary>Gets the accent CSS color used for primary actions.</summary>
    public string Accent { get; }

    /// <summary>Gets the corner radius as a CSS length.</summary>
    public string Radius { get; }

    /// <summary>Gets the padding/gap spacing as a CSS length.</summary>
    public string Spacing { get; }

    /// <summary>Gets the focus-visible ring CSS color.</summary>
    public string FocusRing { get; }

    private static string RequireValue(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}

namespace SyntaxCircus.FancyBlazor;

/// <summary>A four-color palette shared by FancyBlazor effects.</summary>
public sealed record FancyPalette
{
    /// <summary>Creates a palette from CSS color values.</summary>
    public FancyPalette(string primary, string secondary, string accent, string background)
    {
        Primary = RequireColor(primary, nameof(primary));
        Secondary = RequireColor(secondary, nameof(secondary));
        Accent = RequireColor(accent, nameof(accent));
        Background = RequireColor(background, nameof(background));
    }

    /// <summary>Gets the primary CSS color.</summary>
    public string Primary { get; }

    /// <summary>Gets the secondary CSS color.</summary>
    public string Secondary { get; }

    /// <summary>Gets the accent CSS color.</summary>
    public string Accent { get; }

    /// <summary>Gets the background CSS color.</summary>
    public string Background { get; }

    private static string RequireColor(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}

namespace SyntaxCircus.FancyBlazor;

/// <summary>Built-in themes tuned for the FancyBlazor UI companion.</summary>
public static class FancyUiThemes
{
    /// <summary>A dark, Witchlight-accented default theme.</summary>
    public static FancyUiTheme Default { get; } = new(
        surface: "#0f172a",
        text: "#e2e8f0",
        border: "rgba(226, 232, 240, 0.16)",
        accent: FancyPalettes.Witchlight.Primary,
        radius: "0.75rem",
        spacing: "0.75rem 1.25rem",
        focusRing: FancyPalettes.Witchlight.Accent);
}

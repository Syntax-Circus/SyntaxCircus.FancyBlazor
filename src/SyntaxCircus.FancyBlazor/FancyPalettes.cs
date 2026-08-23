namespace SyntaxCircus.FancyBlazor;

/// <summary>Built-in palettes tuned for decorative effects.</summary>
public static class FancyPalettes
{
    /// <summary>Deep blue with cool cyan highlights.</summary>
    public static FancyPalette Midnight { get; } = new("#17315c", "#445ca8", "#87d7e8", "#07111f");

    /// <summary>Violet, blue, and nacreous green over ink.</summary>
    public static FancyPalette Witchlight { get; } = new("#5e82f6", "#a855f7", "#22d3ee", "#08111f");

    /// <summary>Warm orange and rose over dark brown.</summary>
    public static FancyPalette Ember { get; } = new("#f97316", "#fb7185", "#fbbf24", "#21100b");

    /// <summary>Cold blue and silver over deep navy.</summary>
    public static FancyPalette Glacier { get; } = new("#60a5fa", "#a5f3fc", "#e2e8f0", "#071827");

    /// <summary>Emerald and sea-glass tones over forest black.</summary>
    public static FancyPalette Viridian { get; } = new("#10b981", "#2dd4bf", "#a7f3d0", "#071914");
}

namespace SyntaxCircus.FancyBlazor;

/// <summary>Controls the pixel-density ceiling used by GPU effects.</summary>
public enum FancyQuality
{
    /// <summary>Use the balanced package default.</summary>
    Auto,

    /// <summary>Cap rendering at one device pixel per CSS pixel.</summary>
    Low,

    /// <summary>Cap rendering at 1.5 device pixels per CSS pixel.</summary>
    Medium,

    /// <summary>Cap rendering at two device pixels per CSS pixel.</summary>
    High,
}

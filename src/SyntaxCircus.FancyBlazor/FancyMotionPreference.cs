namespace SyntaxCircus.FancyBlazor;

/// <summary>Controls how FancyBlazor responds to reduced-motion preferences.</summary>
public enum FancyMotionPreference
{
    /// <summary>Follow the browser's <c>prefers-reduced-motion</c> setting.</summary>
    RespectSystem,

    /// <summary>Always render reduced/static effects.</summary>
    AlwaysReduce,

    /// <summary>Allow motion even when the browser requests reduced motion.</summary>
    IgnoreSystem,
}

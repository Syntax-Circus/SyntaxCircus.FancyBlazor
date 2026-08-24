namespace SyntaxCircus.FancyBlazor;

/// <summary>Controls resource limits for optional FancyBlazor WebGL effects.</summary>
public sealed class FancyWebGlOptions
{
    private int _maxActiveContexts = 4;

    /// <summary>Gets or sets the maximum number of concurrently active WebGL contexts, clamped from one through eight.</summary>
    public int MaxActiveContexts
    {
        get => _maxActiveContexts;
        set => _maxActiveContexts = Math.Clamp(value, 1, 8);
    }
}

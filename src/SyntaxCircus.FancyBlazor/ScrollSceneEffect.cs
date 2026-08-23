namespace SyntaxCircus.FancyBlazor;

/// <summary>Continuous viewport treatments supported by <see cref="ScrollScene"/>.</summary>
public enum ScrollSceneEffect
{
    /// <summary>Subtly adjusts opacity through the viewport.</summary>
    Fade,

    /// <summary>Moves content vertically through the viewport.</summary>
    Lift,

    /// <summary>Applies a subtle blur away from the viewport center.</summary>
    Blur,
}

namespace SyntaxCircus.FancyBlazor;

/// <summary>Entry transitions supported by <see cref="Reveal"/>.</summary>
public enum RevealEffect
{
    /// <summary>Fade without translation.</summary>
    Fade,

    /// <summary>Fade while moving upward into place.</summary>
    FadeUp,

    /// <summary>Fade, move upward, and remove blur.</summary>
    BlurUp,
}

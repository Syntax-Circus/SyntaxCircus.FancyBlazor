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

    /// <summary>Fade while sliding in from the left.</summary>
    SlideFromLeft,

    /// <summary>Fade while sliding in from the right.</summary>
    SlideFromRight,

    /// <summary>Fade while sliding down from above.</summary>
    SlideFromTop,

    /// <summary>Fade while sliding up from below.</summary>
    SlideFromBottom,
}

# Narrative motion

Narrative motion is for a small number of long-form semantic sections. `ScrollScene`, `ScrollIndicator`, and `ScrollBackdrop` use local viewport progress; they never pin a section, change scroll position, or make content unavailable before interactivity.

Use `ScrollScene` for the semantic chapter, `ScrollBackdrop` for an atmosphere layer, and `ScrollIndicator` only when a local visual reading cue helps. Keep them out of dense lists and avoid stacking many scroll-driven wrappers.

Progress is calculated only while a wrapper is intersecting. Browser work is batched in one animation frame per scroll/resize event and stops while the document is hidden, while the wrapper is offscreen, and on disposal. Reduced motion keeps content static and useful.

[Compiling demo](../../samples/FancyBlazor.Demo.Client/Pages/NarrativeMotion.razor)

# Interaction feedback

Interaction feedback decorates a consumer-owned control; it never supplies the control's behavior. Use `HoverLift` for fine-pointer warmth, `PressScale` for short pointer and keyboard activation feedback, and `FocusHalo` to supplement—not replace—the browser focus outline.

Wrappers add no roles or tab stops. Hover is limited to fine pointers, press listeners never cancel activation, and FocusHalo appears for pointer, touch, or keyboard focus while preserving the native focus outline. Reduced motion disables transforms and transitions while retaining the useful focus state.

[Compiling demo](../../samples/FancyBlazor.Demo.Client/Pages/InteractionFeedback.razor)

namespace NCrunch.Framework;

/// <summary>
/// Instructs NCrunch to execute a fixture's tests in one task rather than
/// repeatedly paying for shared browser-host setup.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class AtomicAttribute : Attribute;

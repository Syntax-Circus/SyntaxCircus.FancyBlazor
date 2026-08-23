using System.Globalization;

namespace SyntaxCircus.FancyBlazor;

internal static class AttributeComposer
{
    internal static IReadOnlyDictionary<string, object> Compose(
        string stableClass,
        string? cssClass,
        string? generatedStyle,
        string? style,
        IReadOnlyDictionary<string, object>? additionalAttributes,
        IReadOnlyDictionary<string, object>? fixedAttributes = null)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (fixedAttributes is not null)
        {
            foreach (var attribute in fixedAttributes)
            {
                result[attribute.Key] = attribute.Value;
            }
        }

        var additionalClass = GetString(additionalAttributes, "class");
        result["class"] = JoinNonEmpty(" ", stableClass, cssClass, additionalClass);

        var additionalStyle = GetString(additionalAttributes, "style");
        var mergedStyle = JoinNonEmpty(";", TrimTerminator(generatedStyle), TrimTerminator(style), TrimTerminator(additionalStyle));
        if (!string.IsNullOrWhiteSpace(mergedStyle))
        {
            result["style"] = string.Create(CultureInfo.InvariantCulture, $"{mergedStyle};");
        }

        if (additionalAttributes is not null)
        {
            foreach (var attribute in additionalAttributes)
            {
                if (!attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase)
                    && !attribute.Key.Equals("style", StringComparison.OrdinalIgnoreCase))
                {
                    result[attribute.Key] = attribute.Value;
                }
            }
        }

        return result;
    }

    internal static string Number(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    internal static double Clamp(double value, double minimum, double maximum) =>
        double.IsFinite(value) ? Math.Clamp(value, minimum, maximum) : minimum;

    internal static TimeSpan NonNegative(TimeSpan value) => value < TimeSpan.Zero ? TimeSpan.Zero : value;

    private static string? GetString(IReadOnlyDictionary<string, object>? attributes, string key) =>
        attributes?.FirstOrDefault(pair => pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value?.ToString();

    private static string? TrimTerminator(string? value) => value?.Trim().TrimEnd(';');

    private static string JoinNonEmpty(string separator, params string?[] values) =>
        string.Join(separator, values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
}

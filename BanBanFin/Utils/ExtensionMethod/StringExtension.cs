using System.Globalization;

namespace BanBanFin.Utils.ExtensionMethod;

public static class StringExtension
{
    public static string ToUpperEx(this string? instance) => (instance != null) ? instance.ToUpperInvariant() : "";

    public static string ToLowerEx(this string? instance) => (instance != null) ? instance.ToLowerInvariant() : "";

    public static string TrimEx(this string? instance) => (instance == null) ? "" : instance.Trim();

    public static int ToInt(this string instance, int defaultValue = 0)
    {
        return int.TryParse(instance, out var result) ? result : defaultValue;
    }

    public static float ToFloat(this string instance) =>
        float.Parse(instance.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture);

    public static bool ContainsEx(this string? instance, string? value) =>
        !string.IsNullOrEmpty(instance) && !string.IsNullOrEmpty(value) && instance.Contains(value);

    public static bool StartsWithEx(this string? instance, string? value) =>
        (instance != null && value != null) && instance.StartsWith(value);
}
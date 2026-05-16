using System.Text.RegularExpressions;

namespace EnvVarManager.Core;

public static partial class EnvironmentVariableNameValidator
{
    public static bool IsValid(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && NamePattern().IsMatch(name);
    }

    public static string Normalize(string name)
    {
        var normalized = name.Trim().ToUpperInvariant();
        ThrowIfInvalid(normalized);
        return normalized;
    }

    public static void ThrowIfInvalid(string? name)
    {
        if (!IsValid(name))
        {
            throw new ArgumentException("环境变量名只能包含英文字母、数字和下划线，且不能以数字开头。", nameof(name));
        }
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]{0,127}$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();
}

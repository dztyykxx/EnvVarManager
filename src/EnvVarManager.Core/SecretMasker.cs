namespace EnvVarManager.Core;

public static class SecretMasker
{
    public static string Mask(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Length <= 4)
        {
            return "****";
        }

        if (value.Length <= 10)
        {
            return $"{value[..2]}****{value[^2..]}";
        }

        var prefixLength = GetTokenPrefixLength(value);
        if (prefixLength > 0)
        {
            return $"{value[..prefixLength]}****{value[^2..]}";
        }

        return $"{value[..2]}****{value[^2..]}";
    }

    private static int GetTokenPrefixLength(string value)
    {
        var separatorIndex = value.IndexOfAny(['-', '_']);
        if (separatorIndex is >= 1 and <= 4)
        {
            return separatorIndex + 1;
        }

        return 0;
    }
}

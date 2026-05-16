namespace EnvVarManager.Core;

public sealed record ApiKeyEntry(
    string Name,
    string DisplayName,
    string Description,
    string Category,
    bool IsKnown,
    bool IsSet,
    string MaskedValue,
    int ValueLength);

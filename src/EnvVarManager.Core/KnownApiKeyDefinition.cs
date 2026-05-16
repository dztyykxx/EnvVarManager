namespace EnvVarManager.Core;

public sealed record KnownApiKeyDefinition(
    string Name,
    string DisplayName,
    string Description,
    string Category);

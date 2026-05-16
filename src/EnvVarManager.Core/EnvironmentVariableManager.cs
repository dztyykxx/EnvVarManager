namespace EnvVarManager.Core;

public sealed class EnvironmentVariableManager
{
    private readonly IReadOnlyList<KnownApiKeyDefinition> _definitions;
    private readonly IEnvironmentVariableStore _store;

    public EnvironmentVariableManager(
        IEnvironmentVariableStore store,
        IEnumerable<KnownApiKeyDefinition> definitions)
    {
        _store = store;
        _definitions = definitions.ToArray();
    }

    public ApiKeyEntry GetEntry(string name)
    {
        var normalizedName = EnvironmentVariableNameValidator.Normalize(name);
        var definition = _definitions.FirstOrDefault(
            x => string.Equals(x.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        return BuildEntry(
            normalizedName,
            definition?.DisplayName ?? normalizedName,
            definition?.Description ?? "自定义环境变量",
            definition?.Category ?? "Custom",
            definition is not null);
    }

    public IReadOnlyList<ApiKeyEntry> BuildEntries(IEnumerable<string> customNames)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var entries = new List<ApiKeyEntry>();

        foreach (var definition in _definitions)
        {
            names.Add(definition.Name);
            entries.Add(BuildEntry(
                definition.Name,
                definition.DisplayName,
                definition.Description,
                definition.Category,
                isKnown: true));
        }

        foreach (var name in customNames)
        {
            var normalizedName = EnvironmentVariableNameValidator.Normalize(name);
            if (!names.Add(normalizedName))
            {
                continue;
            }

            entries.Add(BuildEntry(
                normalizedName,
                normalizedName,
                _store.GetValue(normalizedName) is null ? "自定义环境变量" : "当前用户级环境变量",
                "Custom",
                isKnown: false));
        }

        foreach (var name in _store.GetNames())
        {
            var normalizedName = EnvironmentVariableNameValidator.Normalize(name);
            if (!names.Add(normalizedName))
            {
                continue;
            }

            entries.Add(BuildEntry(
                normalizedName,
                normalizedName,
                "当前用户级环境变量",
                "User",
                isKnown: false));
        }

        return entries;
    }

    public void SetValue(string name, string value)
    {
        var normalizedName = EnvironmentVariableNameValidator.Normalize(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("环境变量值不能为空。", nameof(value));
        }

        _store.SetValue(normalizedName, value);
    }

    public string? GetValue(string name)
    {
        var normalizedName = EnvironmentVariableNameValidator.Normalize(name);
        return _store.GetValue(normalizedName);
    }

    public void DeleteValue(string name)
    {
        var normalizedName = EnvironmentVariableNameValidator.Normalize(name);
        _store.DeleteValue(normalizedName);
    }

    private ApiKeyEntry BuildEntry(
        string name,
        string displayName,
        string description,
        string category,
        bool isKnown)
    {
        var value = _store.GetValue(name);

        return new ApiKeyEntry(
            name,
            displayName,
            description,
            category,
            isKnown,
            !string.IsNullOrEmpty(value),
            SecretMasker.Mask(value),
            value?.Length ?? 0);
    }
}

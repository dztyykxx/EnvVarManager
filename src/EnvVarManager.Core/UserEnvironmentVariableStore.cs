namespace EnvVarManager.Core;

public sealed class UserEnvironmentVariableStore : IEnvironmentVariableStore
{
    public string? GetValue(string name)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
    }

    public IReadOnlyList<string> GetNames()
    {
        return Environment
            .GetEnvironmentVariables(EnvironmentVariableTarget.User)
            .Keys
            .OfType<string>()
            .Where(EnvironmentVariableNameValidator.IsValid)
            .Select(EnvironmentVariableNameValidator.Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void SetValue(string name, string value)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
    }

    public void DeleteValue(string name)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.User);
    }
}

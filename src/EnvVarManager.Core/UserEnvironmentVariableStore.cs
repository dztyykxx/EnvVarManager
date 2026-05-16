namespace EnvVarManager.Core;

public sealed class UserEnvironmentVariableStore : IEnvironmentVariableStore
{
    public string? GetValue(string name)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
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

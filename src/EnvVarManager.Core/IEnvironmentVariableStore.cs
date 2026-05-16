namespace EnvVarManager.Core;

public interface IEnvironmentVariableStore
{
    string? GetValue(string name);

    void SetValue(string name, string value);

    void DeleteValue(string name);
}

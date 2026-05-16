namespace EnvVarManager.Core;

public interface IEnvironmentVariableStore
{
    string? GetValue(string name);

    IReadOnlyList<string> GetNames();

    void SetValue(string name, string value);

    void DeleteValue(string name);
}

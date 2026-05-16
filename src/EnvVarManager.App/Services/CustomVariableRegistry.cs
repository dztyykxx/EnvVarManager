using System.IO;
using System.Text.Json;
using EnvVarManager.Core;

namespace EnvVarManager.App.Services;

public sealed class CustomVariableRegistry
{
    private readonly string _filePath;

    public CustomVariableRegistry()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _filePath = Path.Combine(appData, "EnvVarManager", "custom-variables.json");
    }

    public IReadOnlyList<string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var names = JsonSerializer.Deserialize<List<string?>>(json) ?? [];
            return NormalizeNames(names);
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    public void Save(IEnumerable<string> names)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var normalized = NormalizeNames(names);
        var json = JsonSerializer.Serialize(normalized, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private static string[] NormalizeNames(IEnumerable<string?> names)
    {
        return names
            .OfType<string>()
            .Select(x => x.Trim())
            .Where(EnvironmentVariableNameValidator.IsValid)
            .Select(EnvironmentVariableNameValidator.Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

using System.Runtime.Versioning;
using Microsoft.Win32;

namespace EnvVarManager.Core;

public sealed class UserEnvironmentVariableStore : IEnvironmentVariableStore
{
    private const string EnvironmentSubKeyName = "Environment";

    public string? GetValue(string name)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        if (!OperatingSystem.IsWindows())
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
        }

        using var environmentKey = OpenEnvironmentKey(writable: false);
        return environmentKey?.GetValue(name)?.ToString();
    }

    public IReadOnlyList<string> GetNames()
    {
        if (!OperatingSystem.IsWindows())
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

        using var environmentKey = OpenEnvironmentKey(writable: false);
        return (environmentKey?.GetValueNames() ?? [])
            .Where(EnvironmentVariableNameValidator.IsValid)
            .Select(EnvironmentVariableNameValidator.Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void SetValue(string name, string value)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        if (!OperatingSystem.IsWindows())
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
            return;
        }

        using var environmentKey = CreateEnvironmentKey();
        environmentKey.SetValue(name, value, GetValueKind(environmentKey, name));
        WindowsEnvironmentChangeNotifier.Queue();
    }

    public void DeleteValue(string name)
    {
        EnvironmentVariableNameValidator.ThrowIfInvalid(name);
        if (!OperatingSystem.IsWindows())
        {
            Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.User);
            return;
        }

        using var environmentKey = OpenEnvironmentKey(writable: true);
        environmentKey?.DeleteValue(name, throwOnMissingValue: false);
        WindowsEnvironmentChangeNotifier.Queue();
    }

    [SupportedOSPlatform("windows")]
    private static RegistryKey? OpenEnvironmentKey(bool writable)
    {
        return Registry.CurrentUser.OpenSubKey(EnvironmentSubKeyName, writable);
    }

    [SupportedOSPlatform("windows")]
    private static RegistryKey CreateEnvironmentKey()
    {
        return Registry.CurrentUser.CreateSubKey(EnvironmentSubKeyName, writable: true)
            ?? throw new InvalidOperationException("无法打开当前用户环境变量注册表项。");
    }

    [SupportedOSPlatform("windows")]
    private static RegistryValueKind GetValueKind(RegistryKey environmentKey, string name)
    {
        try
        {
            return environmentKey.GetValueKind(name) == RegistryValueKind.ExpandString
                ? RegistryValueKind.ExpandString
                : RegistryValueKind.String;
        }
        catch (IOException)
        {
            return RegistryValueKind.String;
        }
    }
}

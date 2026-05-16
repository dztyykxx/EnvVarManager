using EnvVarManager.Core;

namespace EnvVarManager.Core.Tests;

public sealed class EnvironmentVariableManagerTests
{
    [Theory]
    [InlineData("OPENAI_API_KEY")]
    [InlineData("_CUSTOM_TOKEN")]
    [InlineData("KEY_123")]
    public void ValidateNameAcceptsEnvironmentStyleNames(string name)
    {
        Assert.True(EnvironmentVariableNameValidator.IsValid(name));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("OPENAI-API-KEY")]
    [InlineData("OPENAI API KEY")]
    [InlineData("OPENAI=API_KEY")]
    [InlineData("1OPENAI_API_KEY")]
    public void ValidateNameRejectsUnsafeNames(string name)
    {
        Assert.False(EnvironmentVariableNameValidator.IsValid(name));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("abcd", "****")]
    [InlineData("sk-1234567890abcdef", "sk-****ef")]
    [InlineData("ghp_1234567890abcdef", "ghp_****ef")]
    [InlineData("1234567890", "12****90")]
    public void MaskValueKeepsOnlySmallHints(string value, string expected)
    {
        Assert.Equal(expected, SecretMasker.Mask(value));
    }

    [Fact]
    public void SetValueStoresAndReturnsMaskedEntry()
    {
        var store = new InMemoryEnvironmentVariableStore();
        var manager = new EnvironmentVariableManager(store, KnownApiKeyCatalog.DefaultDefinitions);

        manager.SetValue("OPENAI_API_KEY", "sk-1234567890abcdef");

        var entry = manager.GetEntry("OPENAI_API_KEY");

        Assert.True(entry.IsSet);
        Assert.Equal("OPENAI_API_KEY", entry.Name);
        Assert.Equal("sk-****ef", entry.MaskedValue);
        Assert.Equal(19, entry.ValueLength);
    }

    [Fact]
    public void GetValueReturnsStoredSecretForExplicitActions()
    {
        var store = new InMemoryEnvironmentVariableStore();
        var manager = new EnvironmentVariableManager(store, KnownApiKeyCatalog.DefaultDefinitions);

        manager.SetValue("openai_api_key", "sk-1234567890abcdef");

        Assert.Equal("sk-1234567890abcdef", manager.GetValue("OPENAI_API_KEY"));
    }

    [Fact]
    public void DeleteValueRemovesStoredValue()
    {
        var store = new InMemoryEnvironmentVariableStore();
        var manager = new EnvironmentVariableManager(store, KnownApiKeyCatalog.DefaultDefinitions);

        manager.SetValue("DASHSCOPE_API_KEY", "dashscope-secret");
        manager.DeleteValue("DASHSCOPE_API_KEY");

        var entry = manager.GetEntry("DASHSCOPE_API_KEY");

        Assert.False(entry.IsSet);
        Assert.Equal("", entry.MaskedValue);
        Assert.Equal(0, entry.ValueLength);
    }

    [Fact]
    public void BuildEntriesKeepsKnownOrderAndAddsCustomNames()
    {
        var store = new InMemoryEnvironmentVariableStore();
        var catalog = new[]
        {
            new KnownApiKeyDefinition("OPENAI_API_KEY", "OpenAI", "OpenAI API Key", "LLM"),
            new KnownApiKeyDefinition("ANTHROPIC_API_KEY", "Anthropic", "Anthropic API Key", "LLM"),
        };
        var manager = new EnvironmentVariableManager(store, catalog);

        var entries = manager.BuildEntries(["CUSTOM_TOKEN", "OPENAI_API_KEY"]).ToArray();

        Assert.Equal(["OPENAI_API_KEY", "ANTHROPIC_API_KEY", "CUSTOM_TOKEN"], entries.Select(x => x.Name));
    }

    private sealed class InMemoryEnvironmentVariableStore : IEnvironmentVariableStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

        public string? GetValue(string name)
        {
            return _values.GetValueOrDefault(name);
        }

        public void SetValue(string name, string value)
        {
            _values[name] = value;
        }

        public void DeleteValue(string name)
        {
            _values.Remove(name);
        }
    }
}

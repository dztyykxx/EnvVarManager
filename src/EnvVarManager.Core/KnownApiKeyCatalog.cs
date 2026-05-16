namespace EnvVarManager.Core;

public static class KnownApiKeyCatalog
{
    public static IReadOnlyList<KnownApiKeyDefinition> DefaultDefinitions { get; } =
    [
        new("OPENAI_API_KEY", "OpenAI", "OpenAI 官方 API Key", "LLM"),
        new("OPENAI_BASE_URL", "OpenAI Base URL", "OpenAI 兼容接口地址，可选", "LLM"),
        new("ANTHROPIC_API_KEY", "Anthropic", "Claude API Key", "LLM"),
        new("GEMINI_API_KEY", "Gemini", "Google Gemini API Key", "LLM"),
        new("GOOGLE_API_KEY", "Google API", "Google AI 或其他 Google API Key", "LLM"),
        new("DASHSCOPE_API_KEY", "DashScope", "阿里云百炼 / 通义千问 API Key", "LLM"),
        new("DEEPSEEK_API_KEY", "DeepSeek", "DeepSeek API Key", "LLM"),
        new("OPENROUTER_API_KEY", "OpenRouter", "OpenRouter API Key", "LLM"),
        new("GROQ_API_KEY", "Groq", "Groq API Key", "LLM"),
        new("MISTRAL_API_KEY", "Mistral", "Mistral API Key", "LLM"),
        new("COHERE_API_KEY", "Cohere", "Cohere API Key", "LLM"),
        new("PERPLEXITY_API_KEY", "Perplexity", "Perplexity API Key", "LLM"),
        new("XAI_API_KEY", "xAI", "xAI API Key", "LLM"),
        new("AZURE_OPENAI_API_KEY", "Azure OpenAI", "Azure OpenAI API Key", "Azure"),
        new("AZURE_OPENAI_ENDPOINT", "Azure Endpoint", "Azure OpenAI Endpoint", "Azure"),
        new("GITHUB_TOKEN", "GitHub Token", "GitHub Personal Access Token", "Dev"),
        new("HF_TOKEN", "Hugging Face", "Hugging Face Token", "Dev"),
        new("LANGSMITH_API_KEY", "LangSmith", "LangSmith API Key", "Observability"),
        new("TAVILY_API_KEY", "Tavily", "Tavily Search API Key", "Search"),
        new("SERPAPI_API_KEY", "SerpApi", "SerpApi API Key", "Search"),
        new("FIRECRAWL_API_KEY", "Firecrawl", "Firecrawl API Key", "Search"),
    ];
}

using EnvVarManager.Core;

namespace EnvVarManager.App.ViewModels;

public sealed class ApiKeyRowViewModel
{
    public ApiKeyRowViewModel(ApiKeyEntry entry, int displayOrder)
    {
        Name = entry.Name;
        DisplayName = entry.DisplayName;
        Description = entry.Description;
        Category = entry.Category;
        IsKnown = entry.IsKnown;
        IsSet = entry.IsSet;
        MaskedValue = string.IsNullOrEmpty(entry.MaskedValue) ? "未设置" : entry.MaskedValue;
        ValueLength = entry.ValueLength;
        DisplayOrder = displayOrder;
    }

    public string Name { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public string Category { get; }

    public bool IsKnown { get; }

    public bool IsSet { get; }

    public string MaskedValue { get; }

    public int ValueLength { get; }

    public int DisplayOrder { get; }

    public string StatusText => IsSet ? "已设置" : "未设置";

    public string DetailText => IsSet ? $"已设置，长度 {ValueLength}" : "未设置";
}

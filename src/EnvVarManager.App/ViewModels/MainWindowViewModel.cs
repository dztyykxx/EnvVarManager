using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using EnvVarManager.App.Commands;
using EnvVarManager.App.Services;
using EnvVarManager.Core;

namespace EnvVarManager.App.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly CustomVariableRegistry _customVariableRegistry;
    private readonly EnvironmentVariableManager _manager;
    private readonly ObservableCollection<ApiKeyRowViewModel> _entries = [];
    private string _customName = "";
    private string _draftValue = "";
    private string _revealedCurrentValue = "";
    private string _searchText = "";
    private bool _isCurrentValueRevealed;
    private bool _isBusy;
    private ApiKeyRowViewModel? _selectedEntry;
    private string _statusMessage = "就绪。选择一个变量后可以保存新值或删除用户级环境变量。";

    public MainWindowViewModel()
        : this(
            new EnvironmentVariableManager(
                new UserEnvironmentVariableStore(),
                KnownApiKeyCatalog.DefaultDefinitions),
            new CustomVariableRegistry())
    {
    }

    public MainWindowViewModel(
        EnvironmentVariableManager manager,
        CustomVariableRegistry customVariableRegistry)
    {
        _manager = manager;
        _customVariableRegistry = customVariableRegistry;

        EntriesView = CollectionViewSource.GetDefaultView(_entries);
        EntriesView.Filter = FilterEntry;

        RefreshCommand = new RelayCommand(Refresh);
        AddCustomCommand = new RelayCommand(AddCustomVariable, () => !string.IsNullOrWhiteSpace(CustomName));
        SaveCommand = new RelayCommand(SaveSelectedValueAsync, CanSaveSelectedValue);
        DeleteCommand = new RelayCommand(
            DeleteSelectedValueAsync,
            () => !IsBusy && SelectedEntry is not null && (SelectedEntry.IsSet || !SelectedEntry.IsKnown));
        CopyNameCommand = new RelayCommand(CopySelectedName, () => SelectedEntry is not null);
        ToggleRevealCurrentValueCommand = new RelayCommand(ToggleRevealCurrentValue, CanReadCurrentValue);
        CopyCurrentValueCommand = new RelayCommand(CopyCurrentValue, CanReadCurrentValue);

        LoadEntries();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action? ClearSecretInputRequested;

    public ICollectionView EntriesView { get; }

    public RelayCommand RefreshCommand { get; }

    public RelayCommand AddCustomCommand { get; }

    public RelayCommand SaveCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public RelayCommand CopyNameCommand { get; }

    public RelayCommand ToggleRevealCurrentValueCommand { get; }

    public RelayCommand CopyCurrentValueCommand { get; }

    public ApiKeyRowViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetProperty(ref _selectedEntry, value))
            {
                DraftValue = "";
                HideCurrentValue();
                ClearSecretInputRequested?.Invoke();
                RaiseCommandStates();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                EntriesView.Refresh();
            }
        }
    }

    public string CustomName
    {
        get => _customName;
        set
        {
            if (SetProperty(ref _customName, value))
            {
                AddCustomCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DraftValue
    {
        get => _draftValue;
        set
        {
            if (SetProperty(ref _draftValue, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string CurrentValueText
    {
        get
        {
            if (SelectedEntry is null)
            {
                return "未选择变量";
            }

            if (!SelectedEntry.IsSet)
            {
                return "未设置";
            }

            return _isCurrentValueRevealed ? _revealedCurrentValue : SelectedEntry.MaskedValue;
        }
    }

    public string RevealCurrentValueButtonText => _isCurrentValueRevealed ? "隐藏当前值" : "查看当前值";

    private void Refresh()
    {
        var selectedName = SelectedEntry?.Name;
        LoadEntries(selectedName);
        StatusMessage = "已刷新当前用户级环境变量。";
    }

    private void AddCustomVariable()
    {
        string normalizedName;
        try
        {
            normalizedName = EnvironmentVariableNameValidator.Normalize(CustomName);
        }
        catch (ArgumentException ex)
        {
            StatusMessage = ex.Message;
            return;
        }

        var customNames = _customVariableRegistry.Load().ToList();
        if (!KnownApiKeyCatalog.DefaultDefinitions.Any(x =>
                string.Equals(x.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
            && !customNames.Contains(normalizedName, StringComparer.OrdinalIgnoreCase))
        {
            customNames.Add(normalizedName);
            _customVariableRegistry.Save(customNames);
        }

        CustomName = "";
        LoadEntries(normalizedName);
        StatusMessage = $"已添加 {normalizedName}。";
    }

    private async void SaveSelectedValueAsync()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        var savedName = SelectedEntry.Name;
        var savedValue = DraftValue;
        IsBusy = true;
        StatusMessage = $"正在保存 {savedName}...";

        try
        {
            await Task.Run(() =>
            {
                _manager.SetValue(savedName, savedValue);
                Environment.SetEnvironmentVariable(savedName, savedValue, EnvironmentVariableTarget.Process);
                WindowsEnvironmentChangeNotifier.Notify();
            });

            LoadEntries(savedName);
            StatusMessage = $"已保存 {savedName}。请重新打开终端、IDE 或 Codex 后使用。";
            HideCurrentValue();
            ClearSecretInputRequested?.Invoke();
        }
        catch (ArgumentException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void DeleteSelectedValueAsync()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        var deletedName = SelectedEntry.Name;
        if (!SelectedEntry.IsSet && !SelectedEntry.IsKnown)
        {
            RemoveCustomName(deletedName);
            LoadEntries();
            StatusMessage = $"已从自定义列表移除 {deletedName}。";
            ClearSecretInputRequested?.Invoke();
            return;
        }

        var result = MessageBox.Show(
            $"确定要删除用户级环境变量 {deletedName} 吗？",
            "确认删除",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = $"正在删除 {deletedName}...";

        try
        {
            await Task.Run(() =>
            {
                _manager.DeleteValue(deletedName);
                Environment.SetEnvironmentVariable(deletedName, null, EnvironmentVariableTarget.Process);
                WindowsEnvironmentChangeNotifier.Notify();
            });

            LoadEntries(deletedName);
            StatusMessage = $"已删除 {deletedName}。";
            HideCurrentValue();
            ClearSecretInputRequested?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RemoveCustomName(string name)
    {
        var customNames = _customVariableRegistry.Load()
            .Where(x => !string.Equals(x, name, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        _customVariableRegistry.Save(customNames);
    }

    private void CopySelectedName()
    {
        if (SelectedEntry is null)
        {
            return;
        }

        if (SafeClipboard.TrySetText(SelectedEntry.Name, out var errorMessage))
        {
            StatusMessage = $"已复制变量名 {SelectedEntry.Name}。";
            return;
        }

        StatusMessage = errorMessage;
    }

    private void ToggleRevealCurrentValue()
    {
        if (SelectedEntry is null || !SelectedEntry.IsSet)
        {
            return;
        }

        if (_isCurrentValueRevealed)
        {
            HideCurrentValue();
            StatusMessage = $"已隐藏 {SelectedEntry.Name} 的当前值。";
            return;
        }

        _revealedCurrentValue = _manager.GetValue(SelectedEntry.Name) ?? "";
        _isCurrentValueRevealed = true;
        OnPropertyChanged(nameof(CurrentValueText));
        OnPropertyChanged(nameof(RevealCurrentValueButtonText));
        StatusMessage = $"已显示 {SelectedEntry.Name} 的当前值，请注意不要在录屏或截图中泄露。";
    }

    private void CopyCurrentValue()
    {
        if (SelectedEntry is null || !SelectedEntry.IsSet)
        {
            return;
        }

        var value = _manager.GetValue(SelectedEntry.Name);
        if (string.IsNullOrEmpty(value))
        {
            StatusMessage = $"{SelectedEntry.Name} 当前未设置。";
            return;
        }

        if (SafeClipboard.TrySetText(value, out var errorMessage))
        {
            StatusMessage = $"已复制 {SelectedEntry.Name} 的当前值。";
            return;
        }

        StatusMessage = errorMessage;
    }

    private void HideCurrentValue()
    {
        _isCurrentValueRevealed = false;
        _revealedCurrentValue = "";
        OnPropertyChanged(nameof(CurrentValueText));
        OnPropertyChanged(nameof(RevealCurrentValueButtonText));
    }

    private bool CanSaveSelectedValue()
    {
        return !IsBusy && SelectedEntry is not null && !string.IsNullOrWhiteSpace(DraftValue);
    }

    private void LoadEntries(string? selectName = null)
    {
        var customNames = _customVariableRegistry.Load();
        var entries = _manager.BuildEntries(customNames)
            .Select((entry, index) => new ApiKeyRowViewModel(entry, index))
            .OrderByDescending(x => x.IsSet)
            .ThenBy(x => x.DisplayOrder)
            .ToArray();

        _entries.Clear();
        foreach (var entry in entries)
        {
            _entries.Add(entry);
        }

        EntriesView.Refresh();

        SelectedEntry =
            _entries.FirstOrDefault(x => string.Equals(x.Name, selectName, StringComparison.OrdinalIgnoreCase))
            ?? _entries.FirstOrDefault();
        RaiseCommandStates();
    }

    private bool FilterEntry(object item)
    {
        if (item is not ApiKeyRowViewModel entry)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var query = SearchText.Trim();
        return entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.StatusText.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.DetailText.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void RaiseCommandStates()
    {
        SaveCommand.RaiseCanExecuteChanged();
        DeleteCommand.RaiseCanExecuteChanged();
        CopyNameCommand.RaiseCanExecuteChanged();
        ToggleRevealCurrentValueCommand.RaiseCanExecuteChanged();
        CopyCurrentValueCommand.RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(CurrentValueText));
        OnPropertyChanged(nameof(RevealCurrentValueButtonText));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private bool CanReadCurrentValue()
    {
        return SelectedEntry is not null && SelectedEntry.IsSet;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

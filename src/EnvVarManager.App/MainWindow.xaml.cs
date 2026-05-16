using System.Windows;
using EnvVarManager.App.ViewModels;

namespace EnvVarManager.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        _viewModel.ClearSecretInputRequested += ClearSecretValue;
        DataContext = _viewModel;
    }

    private void SecretValueBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.DraftValue = SecretValueBox.Password;
    }

    private void ClearSecretValue_OnClick(object sender, RoutedEventArgs e)
    {
        ClearSecretValue();
    }

    private void ClearSecretValue()
    {
        if (SecretValueBox.Password.Length > 0)
        {
            SecretValueBox.Clear();
        }

        _viewModel.DraftValue = "";
    }
}

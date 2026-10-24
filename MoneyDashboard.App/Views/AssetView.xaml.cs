using MoneyDashboard.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MoneyDashboard.App.Views;

public partial class AssetView : UserControl
{
    private readonly AssetViewModel _viewModel;

    public AssetView(
        AssetViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += AssetView_Loaded;
    }

    private async void AssetView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}
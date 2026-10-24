using MoneyDashboard.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MoneyDashboard.App.Views;

public partial class IncomeView : UserControl
{
    private readonly IncomeViewModel _viewModel;

    public IncomeView(
        IncomeViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += IncomeView_Loaded;
    }

    private async void IncomeView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}
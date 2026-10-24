using MoneyDashboard.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MoneyDashboard.App.Views;

public partial class ExpenseView : UserControl
{
    private readonly ExpenseViewModel _viewModel;

    public ExpenseView(
        ExpenseViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += ExpenseView_Loaded;
    }

    private async void ExpenseView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}
using MoneyDashboard.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MoneyDashboard.App.Views;

public partial class SubscriptionView : UserControl
{
    private readonly SubscriptionViewModel _viewModel;

    public SubscriptionView(
        SubscriptionViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += SubscriptionView_Loaded;
    }

    private async void SubscriptionView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}
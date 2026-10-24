using MoneyDashboard.App.Services;
using System.Windows;

namespace MoneyDashboard.App;

public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(
        INavigationService navigationService)
    {
        InitializeComponent();

        _navigationService = navigationService;

        _navigationService.Navigated += OnNavigated;

        _navigationService.Navigate("Dashboard");
    }

    private void OnNavigated(object view)
    {
        MainContent.Content = view;
    }

    private void DashboardButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _navigationService.Navigate("Dashboard");
    }

    private void IncomeButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _navigationService.Navigate("Income");
    }

    private void ExpenseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _navigationService.Navigate("Expense");
    }

    private void SubscriptionButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _navigationService.Navigate("Subscription");
    }

    private void AssetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _navigationService.Navigate("Asset");
    }
}
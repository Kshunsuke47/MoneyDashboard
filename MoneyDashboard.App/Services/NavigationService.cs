using Microsoft.Extensions.DependencyInjection;
using MoneyDashboard.App.Views;

namespace MoneyDashboard.App.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public event Action<object>? Navigated;

    public bool IsNavigationLocked { get; set; }

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Navigate(string destination)
    {
        if (IsNavigationLocked)
        {
            return;
        }

        object view = destination switch
        {
            "Dashboard" =>
                _serviceProvider.GetRequiredService<DashboardView>(),

            "Income" =>
                _serviceProvider.GetRequiredService<IncomeView>(),

            "Expense" =>
                _serviceProvider.GetRequiredService<ExpenseView>(),

            "Subscription" =>
                _serviceProvider.GetRequiredService<SubscriptionView>(),

            "Asset" =>
                _serviceProvider.GetRequiredService<AssetView>(),

            _ => throw new ArgumentException(
                $"存在しない画面です: {destination}")
        };

        Navigated?.Invoke(view);
    }
}
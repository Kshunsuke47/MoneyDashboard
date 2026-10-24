namespace MoneyDashboard.App.Services;

public interface INavigationService
{
    event Action<object>? Navigated;

    bool IsNavigationLocked { get; set; }

    void Navigate(string destination);
}
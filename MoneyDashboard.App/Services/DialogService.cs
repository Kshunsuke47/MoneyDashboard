using System.Windows;

namespace MoneyDashboard.App.Services;

public class DialogService : IDialogService
{
    public void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Money Dashboard",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    public bool ShowConfirmation(string message)
    {
        var result = MessageBox.Show(
            message,
            "Money Dashboard",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }
}
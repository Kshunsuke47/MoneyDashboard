namespace MoneyDashboard.App.Services;

public interface IDialogService
{
    void ShowError(string message);

    bool ShowConfirmation(string message);
}
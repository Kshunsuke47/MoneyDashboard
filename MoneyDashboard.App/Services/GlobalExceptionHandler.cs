using Microsoft.Extensions.Logging;
using System.Windows;

namespace MoneyDashboard.App.Services;

public class GlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public void Handle(Exception exception)
    {
        _logger.LogCritical(
            exception,
            "Unhandled exception occurred.");

        MessageBox.Show(
            "予期しないエラーが発生しました。\n" +
            "アプリを再起動してください。",
            "Money Dashboard",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
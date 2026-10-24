using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyDashboard.App.Services;
using MoneyDashboard.App.ViewModels;
using MoneyDashboard.App.Views;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Core.Services;
using MoneyDashboard.Infrastructure.Database;
using MoneyDashboard.Infrastructure.Repositories;
using Serilog;
using System.IO;
using System.Windows;

namespace MoneyDashboard.App;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();

        var appFolderPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MoneyDashboard");

        var logFolderPath =
            Path.Combine(appFolderPath, "logs");

        Directory.CreateDirectory(logFolderPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(
                    logFolderPath,
                    "moneydashboard-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        // DB接続・初期化
        services.AddSingleton<
            ISqliteConnectionFactory,
            SqliteConnectionFactory>();

        services.AddSingleton<DatabaseInitializer>();

        // Repository
        services.AddTransient<
            ICategoryRepository,
            CategoryRepository>();

        services.AddTransient<
            IIncomeRepository,
            IncomeRepository>();

        services.AddTransient<
            IExpenseRepository,
            ExpenseRepository>();

        services.AddTransient<
            ISubscriptionRepository,
            SubscriptionRepository>();

        services.AddTransient<
            IBankAccountRepository,
            BankAccountRepository>();

        services.AddTransient<
            IMonthlyBankBalanceRepository,
            MonthlyBankBalanceRepository>();

        services.AddTransient<
            INisaRepository,
            NisaRepository>();

        // Service
        services.AddTransient<
            IIncomeService,
            IncomeService>();

        services.AddTransient<
            IExpenseService,
            ExpenseService>();

        services.AddTransient<
            ISubscriptionService,
            SubscriptionService>();

        services.AddTransient<
            IAssetService,
            AssetService>();

        services.AddTransient<
            IDashboardService,
            DashboardService>();

        services.AddSingleton<
            IDialogService,
            DialogService>();

        services.AddSingleton<
            GlobalExceptionHandler>();

        //Navigation
        services.AddSingleton<
            INavigationService,
            NavigationService>();

        // ViewModel
        services.AddTransient<
            DashboardViewModel>();

        services.AddTransient<
            IncomeViewModel>();

        services.AddTransient<
            ExpenseViewModel>();

        services.AddTransient<
            SubscriptionViewModel>();

        services.AddTransient<
            AssetViewModel>();

        // View
        services.AddTransient<
            DashboardView>();

        services.AddTransient<
            IncomeView>();

        services.AddTransient<
            ExpenseView>();

        services.AddTransient<
            SubscriptionView>();

        services.AddTransient<
            AssetView>();

        // Window
        services.AddTransient<
            MainWindow>();

        _serviceProvider =
            services.BuildServiceProvider();

        var exceptionHandler =
            _serviceProvider
            .GetRequiredService<GlobalExceptionHandler>();

        DispatcherUnhandledException +=
            (sender, e) =>
            {
                exceptionHandler.Handle(e.Exception);

                e.Handled = true;
            };
    }

    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var databaseInitializer =
                _serviceProvider
                    .GetRequiredService<DatabaseInitializer>();

            await databaseInitializer.InitializeAsync();

            var mainWindow =
                _serviceProvider
                    .GetRequiredService<MainWindow>();

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            var logger =
                _serviceProvider
                    .GetRequiredService<ILogger<App>>();

            logger.LogCritical(
                ex,
                "Application startup failed.");

            MessageBox.Show(
                "アプリの起動に失敗しました。",
                "Money Dashboard",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();

        _serviceProvider.Dispose();

        base.OnExit(e);
    }
}
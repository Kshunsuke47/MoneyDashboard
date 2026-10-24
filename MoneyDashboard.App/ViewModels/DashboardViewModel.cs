using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MoneyDashboard.App.Services;
using MoneyDashboard.App.ViewModels.Items;
using MoneyDashboard.Core.Services;
using System.Collections.ObjectModel;

namespace MoneyDashboard.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DashboardViewModel> _logger;

    public DashboardViewModel(
        IDashboardService dashboardService, ILogger<DashboardViewModel> logger, IDialogService dialogService)
    {
        _dashboardService = dashboardService;
        _dialogService = dialogService;
        _logger = logger;

        var today = DateOnly.FromDateTime(DateTime.Today);

        TargetMonth =
            new DateOnly(today.Year, today.Month, 1);
    }

    [ObservableProperty]
    private DateOnly targetMonth;

    [ObservableProperty]
    private long incomeTotal;

    [ObservableProperty]
    private long expenseTotal;

    [ObservableProperty]
    private long assetIncreaseForecast;

    [ObservableProperty]
    private long? openingBankBalance;

    [ObservableProperty]
    private long? projectedClosingBankBalance;

    [ObservableProperty]
    private long? nisaEvaluationAmount;

    [ObservableProperty]
    private long? totalAssets;

    [ObservableProperty]
    private long activeSubscriptionMonthlyTotal;

    [ObservableProperty]
    private int activeSubscriptionCount;

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<UnexpectedExpenseItem>
        UnexpectedExpenses
    { get; } = new();

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var data =
                await _dashboardService.GetDashboardAsync(
                    TargetMonth);

            IncomeTotal = data.IncomeTotal;
            ExpenseTotal = data.ExpenseTotal;

            AssetIncreaseForecast =
                data.AssetIncreaseForecast;

            OpeningBankBalance =
                data.OpeningBankBalance;

            ProjectedClosingBankBalance =
                data.ProjectedClosingBankBalance;

            NisaEvaluationAmount =
                data.NisaEvaluationAmount;

            TotalAssets =
                data.TotalAssets;

            ActiveSubscriptionMonthlyTotal =
                data.ActiveSubscriptionMonthlyTotal;

            ActiveSubscriptionCount =
                data.ActiveSubscriptionCount;

            UnexpectedExpenses.Clear();

            foreach (var expense in data.UnexpectedExpenses)
            {
                UnexpectedExpenses.Add(
                    new UnexpectedExpenseItem
                    {
                        ExpenseDate =
                            expense.ExpenseDate,

                        Description =
                            expense.Description,

                        Amount =
                            expense.Amount
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load dashboard. TargetMonth: {TargetMonth}",
                TargetMonth.ToString("yyyy-MM"));

            _dialogService.ShowError(
                "Dashboardの取得に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        TargetMonth =
            TargetMonth.AddMonths(-1);

        await RefreshAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        TargetMonth =
            TargetMonth.AddMonths(1);

        await RefreshAsync();
    }
}
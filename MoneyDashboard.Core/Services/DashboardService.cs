using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;

namespace MoneyDashboard.Core.Services;

public class DashboardService : IDashboardService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IAssetService _assetService;

    public DashboardService(IIncomeRepository incomeRepository, IExpenseRepository expenseRepository, ISubscriptionService subscriptionService, IAssetService assetService)
    {
        _incomeRepository = incomeRepository;
        _expenseRepository = expenseRepository;
        _subscriptionService = subscriptionService;
        _assetService = assetService;
    }

    public async Task<DashboardData> GetDashboardAsync(DateOnly targetMonth)
    {
        var incomeTotal = await _incomeRepository.GetTotalByMonthAsync(targetMonth);

        var expenseTotal = await _expenseRepository.GetTotalByMonthAsync(targetMonth);

        var unexpectedExpenses = await _expenseRepository.GetRecentUnexpectedAsync(targetMonth, 5);

        var assetData = await _assetService.GetAssetDataAsync(targetMonth);

        var activeSubscriptionMonthlyTotal = await _subscriptionService.GetActiveMonthlyTotalAsync();

        var activeSubscriptionCount = await _subscriptionService.GetActiveCountAsync();

        var assetIncreaseForecast = incomeTotal - expenseTotal;

        long? projectedClosingBankBalance = assetData.OpeningBankBalance.HasValue ? assetData.OpeningBankBalance.Value + assetIncreaseForecast : null;

        return new DashboardData
        {
            IncomeTotal = incomeTotal,
            ExpenseTotal = expenseTotal,
            AssetIncreaseForecast = assetIncreaseForecast,

            OpeningBankBalance = assetData.OpeningBankBalance,
            ProjectedClosingBankBalance = projectedClosingBankBalance,
            NisaEvaluationAmount = assetData.NisaEvaluationAmount,
            TotalAssets = assetData.TotalAssets,
            ActiveSubscriptionMonthlyTotal = activeSubscriptionMonthlyTotal,
            ActiveSubscriptionCount = activeSubscriptionCount,
            UnexpectedExpenses = unexpectedExpenses,
        };
    }
}
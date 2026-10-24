namespace MoneyDashboard.Core.Models;

public class DashboardData
{
    public long IncomeTotal { get; set; }
    public long ExpenseTotal { get; set; }
    public long AssetIncreaseForecast { get; set; }

    public long? OpeningBankBalance { get; set; }
    public long? ProjectedClosingBankBalance { get; set; }

    public long? NisaEvaluationAmount { get; set; }
    public long? TotalAssets { get; set; }

    public long ActiveSubscriptionMonthlyTotal { get; set; }
    public int ActiveSubscriptionCount { get; set; }

    public IReadOnlyList<Expense> UnexpectedExpenses { get; set; }
        = Array.Empty<Expense>();
}
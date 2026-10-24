namespace MoneyDashboard.Core.Models;

public class MonthlyBankBalance
{
    public long MonthlyBalanceId { get; set; }
    public long AccountId { get; set; }
    public DateOnly TargetMonth { get; set; }
    public long OpeningBalance { get; set; }
}
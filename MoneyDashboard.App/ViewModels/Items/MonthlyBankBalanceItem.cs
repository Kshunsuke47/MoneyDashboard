namespace MoneyDashboard.App.ViewModels.Items;

public class MonthlyBankBalanceItem
{
    public long AccountId { get; set; }

    public string AccountName { get; set; }
        = string.Empty;

    public long? OpeningBalance { get; set; }

    public bool IsActive { get; set; }

    public string StatusText =>
        IsActive ? "利用中" : "利用終了";
}
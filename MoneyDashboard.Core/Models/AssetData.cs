namespace MoneyDashboard.Core.Models;

public class AssetData
{
    public IReadOnlyList<BankAccount> BankAccounts { get; set; }
        = Array.Empty<BankAccount>();

    public IReadOnlyList<MonthlyBankBalance> MonthlyBalances { get; set; }
        = Array.Empty<MonthlyBankBalance>();

    public long? OpeningBankBalance { get; set; }
    public long? NisaEvaluationAmount { get; set; }
    public long? TotalAssets { get; set; }
}
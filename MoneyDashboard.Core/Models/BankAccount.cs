namespace MoneyDashboard.Core.Models;

public class BankAccount
{
    public long AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
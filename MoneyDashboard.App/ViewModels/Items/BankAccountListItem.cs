namespace MoneyDashboard.App.ViewModels.Items;

public class BankAccountListItem
{
    public long AccountId { get; set; }

    public string AccountName { get; set; }
        = string.Empty;

    public bool IsActive { get; set; }
}
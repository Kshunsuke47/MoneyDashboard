namespace MoneyDashboard.App.ViewModels.Items;

public class ExpenseListItem
{
    public long ExpenseId { get; set; }

    public DateOnly ExpenseDate { get; set; }

    public string Description { get; set; }
        = string.Empty;

    public string CategoryName { get; set; }
        = string.Empty;

    public long Amount { get; set; }

    public string? Note { get; set; }

    public bool IsUnexpected { get; set; }

    public string ExpenseTypeText => IsUnexpected ? "急な出費" : "通常";
}
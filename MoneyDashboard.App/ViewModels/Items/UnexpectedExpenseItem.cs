namespace MoneyDashboard.App.ViewModels.Items;

public class UnexpectedExpenseItem
{
    public DateOnly ExpenseDate { get; set; }

    public string Description { get; set; }
        = string.Empty;

    public long Amount { get; set; }
}
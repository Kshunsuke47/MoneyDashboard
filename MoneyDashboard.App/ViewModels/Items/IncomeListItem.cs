namespace MoneyDashboard.App.ViewModels.Items;

public class IncomeListItem
{
    public long IncomeId { get; set; }

    public DateOnly IncomeDate { get; set; }

    public string Description { get; set; }
        = string.Empty;

    public string CategoryName { get; set; }
    = string.Empty;

    public long Amount { get; set; }

    public string? Note { get; set; }
}
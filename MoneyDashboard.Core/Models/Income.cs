namespace MoneyDashboard.Core.Models;

public class Income
{
    public long IncomeId { get; set; }
    public DateOnly IncomeDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public long Amount { get; set; }
    public string? Note { get; set; }
}

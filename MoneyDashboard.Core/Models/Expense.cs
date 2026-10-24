namespace MoneyDashboard.Core.Models;

public class Expense
{
    public long ExpenseId { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public long Amount { get; set; }
    public string? Note { get; set; }
    public bool IsUnexpected { get; set; }
}
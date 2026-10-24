using MoneyDashboard.Core.Enums;

namespace MoneyDashboard.Core.Models;

public class Category
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
}
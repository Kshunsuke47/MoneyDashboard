namespace MoneyDashboard.Core.Models;

public class MonthlyNisaValue
{
    public long NisaValueId { get; set; }
    public DateOnly TargetMonth { get; set; }
    public long EvaluationAmount { get; set; }
}
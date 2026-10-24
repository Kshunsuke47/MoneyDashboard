using MoneyDashboard.Core.Enums;

namespace MoneyDashboard.Core.Models;

public class Subscription
{
    public long SubscriptionId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public long PaymentAmount { get; set; }
    public PaymentCycle PaymentCycle { get; set; }
    public int? PaymentMonth { get; set; }
    public int PaymentDay { get; set; }
    public SubscriptionStatus Status { get; set; }
    public string? Note { get; set; }
}
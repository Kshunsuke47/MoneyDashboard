using MoneyDashboard.Core.Enums;

namespace MoneyDashboard.App.ViewModels.Items;

public class SubscriptionListItem
{
    public long SubscriptionId { get; set; }

    public string ServiceName { get; set; }
        = string.Empty;

    public string CategoryName { get; set; }
        = string.Empty;

    public long PaymentAmount { get; set; }

    public PaymentCycle PaymentCycle { get; set; }

    public string PaymentCycleText =>
    PaymentCycle == PaymentCycle.Monthly
        ? "月払い"
        : "年払い";

    public string StatusText => Status switch
    {
        SubscriptionStatus.Active => "利用中",
        SubscriptionStatus.Paused => "停止中",
        SubscriptionStatus.Cancelled => "解約済み",
        _ => "不明"
    };

    public string PaymentDateText =>
        PaymentCycle == PaymentCycle.Monthly
            ? $"毎月{PaymentDay}日"
            : $"{PaymentMonth}月{PaymentDay}日";

    public int? PaymentMonth { get; set; }

    public int PaymentDay { get; set; }

    public long MonthlyEquivalentAmount { get; set; }

    public SubscriptionStatus Status { get; set; }

    public string? Note { get; set; }
}
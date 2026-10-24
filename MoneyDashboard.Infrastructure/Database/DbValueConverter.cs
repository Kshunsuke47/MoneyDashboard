using MoneyDashboard.Core.Enums;

namespace MoneyDashboard.Infrastructure.Database;

public static class DbValueConverter
{
    public static string ToDateString(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    public static DateOnly ToDateOnly(string value)
    {
        return DateOnly.ParseExact(value, "yyyy-MM-dd");
    }

    public static string ToTargetMonthString(DateOnly targetMonth)
    {
        return targetMonth.ToString("yyyy-MM");
    }

    public static DateOnly ToTargetMonth(string value)
    {
        return DateOnly.ParseExact(
            $"{value}-01", "yyyy-MM-dd");
    }

    public static string ToDbValue(CategoryType value)
    {
        return value switch
        {
            CategoryType.Income => "INCOME",
            CategoryType.Expense => "EXPENSE",
            CategoryType.Subscription => "SUBSCRIPTION",
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public static CategoryType ToCategoryType(string value)
    {
        return value switch
        {
            "INCOME" => CategoryType.Income,
            "EXPENSE" => CategoryType.Expense,
            "SUBSCRIPTION" => CategoryType.Subscription,
            _ => throw new InvalidOperationException(
                $"Unknown category type: {value}")
        };
    }

    public static string ToDbValue(PaymentCycle value)
    {
        return value switch
        {
            PaymentCycle.Monthly => "MONTHLY",
            PaymentCycle.Yearly => "YEARLY",
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public static PaymentCycle ToPaymentCycle(string value)
    {
        return value switch
        {
            "MONTHLY" => PaymentCycle.Monthly,
            "YEARLY" => PaymentCycle.Yearly,
            _ => throw new InvalidOperationException(
                $"Unknown payment cycle: {value}")
        };
    }

    public static string ToDbValue(SubscriptionStatus value)
    {
        return value switch
        {
            SubscriptionStatus.Active => "ACTIVE",
            SubscriptionStatus.Paused => "PAUSED",
            SubscriptionStatus.Cancelled => "CANCELLED",
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };
    }

    public static SubscriptionStatus ToSubscriptionStatus(string value)
    {
        return value switch
        {
            "ACTIVE" => SubscriptionStatus.Active,
            "PAUSED" => SubscriptionStatus.Paused,
            "CANCELLED" => SubscriptionStatus.Cancelled,
            _ => throw new InvalidOperationException(
                $"Unknown subscription status: {value}")
        };
    }

    public static int ToDbValue(bool value)
    {
        return value ? 1 : 0;
    }

    public static bool ToBoolean(long value)
    {
        return value switch
        {
            0 => false,
            1 => true,
            _ => throw new InvalidOperationException(
                $"Unknown boolean value: {value}")
        };
    }
}
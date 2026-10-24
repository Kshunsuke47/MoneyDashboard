using MoneyDashboard.Core.Enums;
using System.Globalization;
using System.Windows.Data;

namespace MoneyDashboard.App.Converters;

public class SubscriptionEnumToTextConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return value switch
        {
            PaymentCycle.Monthly => "月払い",
            PaymentCycle.Yearly => "年払い",

            SubscriptionStatus.Active => "利用中",
            SubscriptionStatus.Paused => "停止中",
            SubscriptionStatus.Cancelled => "解約済み",

            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
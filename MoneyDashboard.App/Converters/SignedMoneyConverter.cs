using System.Globalization;
using System.Windows.Data;

namespace MoneyDashboard.App.Converters;

public class SignedMoneyConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is not long amount)
        {
            return "0円";
        }

        if (amount > 0)
        {
            return $"+{amount:N0}円";
        }

        if (amount < 0)
        {
            return $"{amount:N0}円";
        }

        return "0円";
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
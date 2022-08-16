using CommunityToolkit.Diagnostics;
using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class StatusToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<TransactionsStatus>(value);
		Guard.IsNotNull(value);

        switch ((TransactionsStatus)value)
        {
            case TransactionsStatus.Pending:
                return (Style)App.Current.Resources["Watch"];
            case TransactionsStatus.Sent:
            case TransactionsStatus.Received:
                return (Style)App.Current.Resources["Done"];
            case TransactionsStatus.Failed:
                return (Style)App.Current.Resources["Clear"];
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

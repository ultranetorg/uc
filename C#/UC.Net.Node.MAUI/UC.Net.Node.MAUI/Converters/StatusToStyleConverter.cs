using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class StatusToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<TransactionStatus>(value);
		Guard.IsNotNull(value);

        switch ((TransactionStatus)value)
        {
            case TransactionStatus.Pending:
                return (Style)App.Current.Resources["Watch"];
            case TransactionStatus.Sent:
            case TransactionStatus.Received:
                return (Style)App.Current.Resources["Done"];
            case TransactionStatus.Failed:
                return (Style)App.Current.Resources["Clear"];
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

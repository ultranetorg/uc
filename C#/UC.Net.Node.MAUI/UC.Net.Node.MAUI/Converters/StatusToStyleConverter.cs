using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class StatusToStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Style result = null;
        switch ((TransactionsStatus)value)
        {
            case TransactionsStatus.Pending:
                result = (Style)App.Current.Resources["Watch"];
			break;
            case TransactionsStatus.Sent:
            case TransactionsStatus.Received:
                result = (Style)App.Current.Resources["Done"];
			break;
            case TransactionsStatus.Failed:
                result = (Style)App.Current.Resources["Clear"];
			break;
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

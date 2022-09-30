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
                return "ts_pending_light.png";
            case TransactionStatus.Sent:
            case TransactionStatus.Received:
                return "ts_done_light.png";
            case TransactionStatus.Failed:
                return "ts_failed_light.png";
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

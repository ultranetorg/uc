using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class TransactionStatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		FontImageSource result = null;
        switch ((TransactionsStatus)value)
        {
            case TransactionsStatus.Pending:
                result = new FontImageSource { Color = Settings.Purple, Size = 30, Glyph = IconFont.Accounts, FontFamily = "icomoon" };
			break;
            case TransactionsStatus.Received:
                result = new FontImageSource { Color = Settings.Green, Size = 30, Glyph = IconFont.Receive, FontFamily = "icomoon" };
			break;
            case TransactionsStatus.Sent:
                result = new FontImageSource { Color = Settings.Blue, Size = 30, Glyph = IconFont.Send, FontFamily = "icomoon" };
                break;
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

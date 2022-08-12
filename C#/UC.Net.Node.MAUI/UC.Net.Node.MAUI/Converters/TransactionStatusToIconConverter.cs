using CommunityToolkit.Diagnostics;
using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class TransactionStatusToIconConverter : IValueConverter
{
	private const string _defaultFontFamily = "icomoon";
	private const int _defaultFontSize = 30;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<TransactionsStatus>(value);
		Guard.IsNotNull(value);

		FontImageSource result = null;
        switch ((TransactionsStatus)value)
        {
            case TransactionsStatus.Pending:
                result = new FontImageSource { Color = Settings.Purple, Size = _defaultFontSize, Glyph = IconFont.Accounts, FontFamily = _defaultFontFamily };
			break;
            case TransactionsStatus.Received:
                result = new FontImageSource { Color = Settings.Green, Size = _defaultFontSize, Glyph = IconFont.Receive, FontFamily = _defaultFontFamily };
			break;
            case TransactionsStatus.Sent:
                result = new FontImageSource { Color = Settings.Blue, Size = _defaultFontSize, Glyph = IconFont.Send, FontFamily = _defaultFontFamily };
                break;
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

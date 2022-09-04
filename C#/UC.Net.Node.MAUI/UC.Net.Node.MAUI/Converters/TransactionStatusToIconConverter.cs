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

        switch ((TransactionsStatus)value)
        {
            case TransactionsStatus.Pending:
				// Color source will be changed
                return new FontImageSource { Color = (Color)App.Current.Resources["Purple"], Size = _defaultFontSize, Glyph = IconFont.Accounts, FontFamily = _defaultFontFamily };
            case TransactionsStatus.Received:
                return new FontImageSource { Color = (Color)App.Current.Resources["Green"], Size = _defaultFontSize, Glyph = IconFont.Receive, FontFamily = _defaultFontFamily };
            case TransactionsStatus.Sent:
                return new FontImageSource { Color = (Color)App.Current.Resources["Blue"], Size = _defaultFontSize, Glyph = IconFont.Send, FontFamily = _defaultFontFamily };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

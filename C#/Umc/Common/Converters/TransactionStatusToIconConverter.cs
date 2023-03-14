using System.Globalization;

namespace UC.Umc.Converters;

public class TransactionStatusToIconConverter : IValueConverter
{
	private const string _defaultFontFamily = "icomoon";
	private const int _defaultFontSize = 30;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<TransactionStatus>(value);
		Guard.IsNotNull(value);
		
		FontImageSource imageSource = null;
		var colorsDictionary = App.Current.Resources.MergedDictionaries.First();

		switch ((TransactionStatus)value)
		{
			case TransactionStatus.Pending:
				// Color source will be changed
				imageSource = new FontImageSource { Color = (Color)colorsDictionary["Purple"], Size = _defaultFontSize, Glyph = IconFont.Accounts, FontFamily = _defaultFontFamily };
				break;
			case TransactionStatus.Received:
				imageSource = new FontImageSource { Color = (Color)colorsDictionary["HigherColor"], Size = _defaultFontSize, Glyph = IconFont.Receive, FontFamily = _defaultFontFamily };
				break;
			case TransactionStatus.Sent:
				imageSource = new FontImageSource { Color = (Color)colorsDictionary["Blue"], Size = _defaultFontSize, Glyph = IconFont.Send, FontFamily = _defaultFontFamily };
				break;
			case TransactionStatus.Failed:
				imageSource = new FontImageSource { Color = (Color)colorsDictionary["LowerColor"], Size = _defaultFontSize, Glyph = IconFont.RemoveFromList, FontFamily = _defaultFontFamily };
				break;
			case TransactionStatus.None:
				imageSource = new FontImageSource { Color = (Color)colorsDictionary["LightGray"], Size = _defaultFontSize, Glyph = IconFont.DotsHorizontal, FontFamily = _defaultFontFamily };
				break;
		}

        return imageSource;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

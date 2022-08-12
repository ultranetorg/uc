using CommunityToolkit.Diagnostics;
using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class NotificationTypeConverter : IValueConverter
{
	private const string _defaultFontFamily = "icomoon";
	private const int _defaultFontSize = 40;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<NotificationType>(value);
		Guard.IsNotNull(value);

		FontImageSource result = null;
        switch ((NotificationType) value)
        {
            case NotificationType.ProductOperations:
                result = new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#E03030"),
                    Glyph = IconFont.ProductOperations
                };
			break;
            case NotificationType.SystemEvent:
                result = new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#E09A30"),
                    Glyph = IconFont.SystemEvent
                };
			break;
            case NotificationType.AuthorOperations:
                result = new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.AuthorOperations
                };
			break;
            case NotificationType.TokenOperations:
                result = new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#E03030"),
                    Glyph = IconFont.ProductOperations
                };
			break;
            case NotificationType.Server:
                result = new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.Server
                };
			break;
            case NotificationType.Wallet:
                result = new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.Wallet
                };
                break;
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

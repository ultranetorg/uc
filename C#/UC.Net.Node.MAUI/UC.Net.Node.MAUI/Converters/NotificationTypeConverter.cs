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

        switch ((NotificationType) value)
        {
            case NotificationType.ProductOperations:
                return new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#E03030"),
                    Glyph = IconFont.ProductOperations
                };
            case NotificationType.SystemEvent:
                return new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#E09A30"),
                    Glyph = IconFont.SystemEvent
                };
            case NotificationType.AuthorOperations:
                return new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.AuthorOperations
                };
            case NotificationType.TokenOperations:
                return new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#E03030"),
                    Glyph = IconFont.ProductOperations
                };
            case NotificationType.Server:
                return new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.Server
                };
            case NotificationType.Wallet:
                return new FontImageSource
                {
                    FontFamily = _defaultFontFamily,
                    Size = _defaultFontSize,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.Wallet
                };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

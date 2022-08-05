using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class NotificationTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		FontImageSource result = null;
        switch ((NotificationType) value)
        {
            case NotificationType.ProductOperations:
                result = new FontImageSource
                {
                    FontFamily = "icomoon",
                    Size = 40,
                    Color = Color.FromArgb("#E03030"),
                    Glyph = IconFont.ProductOperations
                };
			break;
            case NotificationType.SystemEvent:
                result = new FontImageSource
                {
                    FontFamily = "icomoon",
                    Size = 40,
                    Color = Color.FromArgb("#E09A30"),
                    Glyph = IconFont.SystemEvent
                };
			break;
            case NotificationType.AuthorOperations:
                result = new FontImageSource
                {
                    FontFamily = "icomoon",
                    Size = 40,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.AuthorOperations
                };
			break;
            case NotificationType.TokenOperations:
                result = new FontImageSource
                {
                    FontFamily = "icomoon",
                    Size = 40,
                    Color = Color.FromArgb("#E03030"),
                    Glyph = IconFont.ProductOperations
                };
			break;
            case NotificationType.Server:
                result = new FontImageSource
                {
                    FontFamily = "icomoon",
                    Size = 40,
                    Color = Color.FromArgb("#53E030"),
                    Glyph = IconFont.Server
                };
			break;
            case NotificationType.Wallet:
                result = new FontImageSource
                {
                    FontFamily = "icomoon",
                    Size = 40,
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

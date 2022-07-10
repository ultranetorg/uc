using System.Globalization;

namespace UC.Net.Node.MAUI.Converters
{
    public class NotificationTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((NotificationType)value)
            {
                case NotificationType.ProductOperations:
                    return new FontImageSource
                    {
                        FontFamily= "icomoon",
                        Size=40,
                        Color=Color.FromArgb("#E03030"),
                        Glyph= IconFont.ProductOperations
                    };
                case NotificationType.SystemEvent:
                    return new FontImageSource
                    {
                        FontFamily = "icomoon",
                        Size = 40,
                        Color = Color.FromArgb("#E09A30"),
                        Glyph = IconFont.SystemEvent
                    };
                case NotificationType.AuthorOperations:
                    return new FontImageSource
                    {
                        FontFamily = "icomoon",
                        Size = 40,
                        Color = Color.FromArgb("#53E030"),
                        Glyph = IconFont.AuthorOperations
                    };
                case NotificationType.TokenOperations:
                    return new FontImageSource
                    {
                        FontFamily = "icomoon",
                        Size = 40,
                        Color = Color.FromArgb("#E03030"),
                        Glyph = IconFont.ProductOperations
                    };
                case NotificationType.Server:
                    return new FontImageSource
                    {
                        FontFamily = "icomoon",
                        Size = 40,
                        Color = Color.FromArgb("#53E030"),
                        Glyph = IconFont.Server
                    };
                case NotificationType.Wallet:
                    return new FontImageSource
                    {
                        FontFamily = "icomoon",
                        Size = 40,
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
}

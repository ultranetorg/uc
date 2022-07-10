using System.Globalization;

namespace UC.Net.Node.MAUI.Converters
{
    public class IconTintConverter : IValueConverter
    {
        Button btn;
        private FontImageSource source;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            btn= (Button)parameter;
            if (btn != null)
            {
                source = btn.ImageSource as FontImageSource;
                source.Color = btn.TextColor;
                return source;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

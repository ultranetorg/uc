using System.Globalization;

namespace UC.Net.Node.MAUI.Converters;

public class IconTintConverter : IValueConverter
{
    private Button btn;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        btn = (Button)parameter;
		FontImageSource result = null;
        if (btn != null)
        {
            result = btn.ImageSource as FontImageSource;
            result.Color = btn.TextColor;
        }
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

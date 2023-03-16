using System.Globalization;

namespace UC.Umc.Converters;

public class IconTintConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<Button>(parameter);
		Guard.IsNotNull(parameter);
		FontImageSource result = null;
		if (parameter is Button btn)
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

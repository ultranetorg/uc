using System.Globalization;

namespace UC.Umc.Common.Converters;

public class IconTintConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		Guard.IsNotNull(parameter);
		Guard.IsOfType<Button>(parameter);

		Button button = (parameter as Button)!;
		FontImageSource? result = button.ImageSource as FontImageSource;
		if (result != null)
		{
			result.Color = button.TextColor;
		}

		return result;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

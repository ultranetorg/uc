using System.Globalization;

namespace UC.Umc.Common.Converters;

public class DecimalToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value != null ? ((decimal)value).ToString(CultureInfo.InvariantCulture) : string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return string.IsNullOrWhiteSpace((string)value) ? 0 : decimal.Parse((string)value);
	}
}

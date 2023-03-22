using System.Globalization;

namespace UC.Umc.Converters;

public class ConnectionToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<NetworkAccess>(value);
		Guard.IsNotNull(value);

        switch ((NetworkAccess)value)
        {
            case NetworkAccess.Internet:
                return "internet_on.png";
			default: return "internet_off.png";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

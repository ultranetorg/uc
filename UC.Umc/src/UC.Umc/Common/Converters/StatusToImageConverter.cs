using System.Globalization;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;

namespace UC.Umc.Common.Converters;

public class StatusToImageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Guard.IsOfType<TransactionStatus>(value);
		Guard.IsNotNull(value);

		string iconName = string.Empty;
		switch ((TransactionStatus)value)
		{
			case TransactionStatus.Pending:
				iconName = $"ts_pending";
				break;
			case TransactionStatus.Sent:
			case TransactionStatus.Received:
				iconName = $"ts_done";
				break;
			case TransactionStatus.Failed:
				iconName = $"ts_failed";
				break;
		}

		return $"{iconName}_{GlobalAppTheme.ThemeLowerCase}.png";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

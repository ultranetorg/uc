using System.Globalization;
using UC.Umc.Models;

namespace UC.Umc.Common.Converters;

public class DomainToImageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Guard.IsOfType<AuthorStatus>(value);
		Guard.IsNotNull(value);

		switch ((AuthorStatus)value)
		{
			case AuthorStatus.Auction:
				return "domain_auction.png";
			case AuthorStatus.Free:
				return "domain_free.png";
			case AuthorStatus.Owned:
				return "domain_personal.png";
			case AuthorStatus.Reserved:
				return "domain_reserved.png";
			case AuthorStatus.Watched:
				return "domain_watch.png";
			default: return string.Empty;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

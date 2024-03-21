using System.Globalization;

namespace UC.Umc.Converters;

public class AuthorToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
		Guard.IsOfType<AuthorStatus>(value);
		Guard.IsNotNull(value);

        switch ((AuthorStatus)value)
        {
            case AuthorStatus.Auction:
                return "author_auction.png";
            case AuthorStatus.Free:
                return "author_free.png";
            case AuthorStatus.Owned:
                return "author_personal.png";
            case AuthorStatus.Reserved:
                return "author_reserved.png";
            case AuthorStatus.Watched:
                return "author_watch.png";
			default: return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

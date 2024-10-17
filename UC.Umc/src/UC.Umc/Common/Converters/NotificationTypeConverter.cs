using System.Globalization;
using UC.Umc.Models;
using UC.Umc.Models.Common;

namespace UC.Umc.Common.Converters;

public class NotificationTypeConverter : IValueConverter
{
	// Need to place fonts and colors in the style resources
	private const string _defaultFontFamily = "icomoon";
	private const int _defaultFontSize = 40;
	private Color _defaultColor1 = Color.FromArgb("#E03030");
	private Color _defaultColor2 = Color.FromArgb("#E09A30");
	private Color _defaultColor3 = Color.FromArgb("#53E030");

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Guard.IsOfType<NotificationType>(value);
		Guard.IsNotNull(value);

		switch ((NotificationType) value)
		{
			case NotificationType.ProductOperations:
				return new FontImageSource
				{
					FontFamily = _defaultFontFamily,
					Size = _defaultFontSize,
					Color = _defaultColor1,
					Glyph = IconFont.ProductOperations
				};
			case NotificationType.SystemEvent:
				return new FontImageSource
				{
					FontFamily = _defaultFontFamily,
					Size = _defaultFontSize,
					Color = _defaultColor2,
					Glyph = IconFont.SystemEvent
				};
			case NotificationType.AuthorOperations:
				return new FontImageSource
				{
					FontFamily = _defaultFontFamily,
					Size = _defaultFontSize,
					Color = _defaultColor3,
					Glyph = IconFont.AuthorOperations
				};
			case NotificationType.TokenOperations:
				return new FontImageSource
				{
					FontFamily = _defaultFontFamily,
					Size = _defaultFontSize,
					Color = _defaultColor1,
					Glyph = IconFont.ProductOperations
				};
			case NotificationType.Server:
				return new FontImageSource
				{
					FontFamily = _defaultFontFamily,
					Size = _defaultFontSize,
					Color = _defaultColor3,
					Glyph = IconFont.Server
				};
			case NotificationType.Wallet:
				return new FontImageSource
				{
					FontFamily = _defaultFontFamily,
					Size = _defaultFontSize,
					Color = _defaultColor3,
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

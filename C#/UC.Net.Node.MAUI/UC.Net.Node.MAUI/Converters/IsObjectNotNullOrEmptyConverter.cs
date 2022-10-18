using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace CMSCOM.Converters;

public class IsObjectNotNullOrEmptyConverter : BaseConverterOneWay<object, bool>
{
    public override bool ConvertFrom(object? value, CultureInfo? culture = null)
        => value != null && !string.IsNullOrWhiteSpace(value as string ?? value.ToString());
}

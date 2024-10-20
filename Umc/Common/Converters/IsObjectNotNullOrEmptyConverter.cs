﻿#nullable enable
using System.Globalization;
using CommunityToolkit.Maui.Converters;

namespace UC.Umc.Converters;

public class IsObjectNotNullOrEmptyConverter : BaseConverterOneWay<object, bool>
{
    public override bool ConvertFrom(object? value, CultureInfo? culture = null)
        => value != null && !string.IsNullOrWhiteSpace(value as string ?? value.ToString());

    public override bool DefaultConvertReturnValue { get; set; } = false;
}

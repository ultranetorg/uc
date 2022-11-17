namespace UC.Umc.Constants;

public static class PopupSizeConstants
{
    // public static Size Small;

    // public static Size Medium;
    public static Size AutoCompleteControl;

    static PopupSizeConstants()
	{
        // examples for fixed sizes
		// Small = new Size(300, 300);

        var displayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;
        var displayHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
        var displayWidth = DeviceDisplay.Current.MainDisplayInfo.Width;

        // examples for relative to screen sizes
		AutoCompleteControl = new Size(0.9 * (displayWidth / displayDensity), 0.45 * (displayHeight / displayDensity));
	}
}

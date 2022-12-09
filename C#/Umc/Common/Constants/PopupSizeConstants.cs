namespace UC.Umc.Constants;

public static class PopupSizeConstants
{
    // public static Size Small;

    public static Size Medium;
	public static Size Large;

	static PopupSizeConstants()
	{
        // examples for fixed sizes
		// Small = new Size(300, 300);

        var displayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;
        var displayHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
        var displayWidth = DeviceDisplay.Current.MainDisplayInfo.Width;

		// examples for relative to screen sizes
		Medium = new Size(displayWidth / displayDensity, 0.45 * (displayHeight / displayDensity));
		Large = new Size(displayWidth / displayDensity, 0.8 * (displayHeight / displayDensity));
	}
}

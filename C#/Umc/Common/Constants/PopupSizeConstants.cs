namespace UC.Umc.Constants;

public static class PopupSizeConstants
{
    public readonly static Size Small;
    public readonly static Size Medium;
	public readonly static Size Large;

	static PopupSizeConstants()
	{
        // examples for fixed sizes
		// Fixed = new Size(300, 300);

        var displayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;
        var displayHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
        var displayWidth = DeviceDisplay.Current.MainDisplayInfo.Width;

		// examples for relative to screen sizes
		Small = new Size(displayWidth / displayDensity, 0.15 * (displayHeight / displayDensity));
		Medium = new Size(displayWidth / displayDensity, 0.45 * (displayHeight / displayDensity));
		Large = new Size(displayWidth / displayDensity, 0.8 * (displayHeight / displayDensity));
	}
}

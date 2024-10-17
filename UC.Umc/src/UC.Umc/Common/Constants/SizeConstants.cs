namespace UC.Umc.Common.Constants;

public static class SizeConstants
{
	public static readonly Size Small;
	public static readonly Size PreMedium;
	public static readonly Size Medium;
	public static readonly Size ExtraMedium;
	public static readonly Size Large;
	public static readonly Size Max;

	public static readonly int SizePerPageMin = 10;
	public static readonly int SizePerPageMed = 25;
	public static readonly int SizePerPageMax = 50;

	static SizeConstants()
	{
		// examples for fixed sizes
		// Fixed = new Size(300, 300);

		var displayDensity = DeviceDisplay.Current.MainDisplayInfo.Density;
		var displayHeight = DeviceDisplay.Current.MainDisplayInfo.Height;
		var displayWidth = DeviceDisplay.Current.MainDisplayInfo.Width;

		// examples for relative to screen sizes
		Small = new Size(displayWidth / displayDensity, 0.15 * (displayHeight / displayDensity));
		PreMedium = new Size(displayWidth / displayDensity, 0.30 * (displayHeight / displayDensity));
		Medium = new Size(displayWidth / displayDensity, 0.45 * (displayHeight / displayDensity));
		ExtraMedium = new Size(displayWidth / displayDensity, 0.60 * (displayHeight / displayDensity));
		Large = new Size(displayWidth / displayDensity, 0.8 * (displayHeight / displayDensity));
		Max = new Size(displayWidth / displayDensity, 0.9 * (displayHeight / displayDensity));
	}
}


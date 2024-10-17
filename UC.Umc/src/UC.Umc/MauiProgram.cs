using CommunityToolkit.Maui;
using UC.Umc.Common.Configurations;
using ZXing.Net.Maui.Controls;

namespace UC.Umc
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseBarcodeReader()
				.UseMauiCommunityToolkit()
				// TODO: review.
				.ConfigureEssentials()
				.ConfigureServices()
				.ConfigureViewModels()
				.ConfigurePages()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("icomoon.ttf", "icomoon");
					fonts.AddFont("Montserrat-Bold.ttf", "Bold");
					fonts.AddFont("Montserrat-Italic.ttf", "Italic");
					fonts.AddFont("Montserrat-Light.ttf", "Light");
					fonts.AddFont("Montserrat-Medium.ttf", "Medium");
					fonts.AddFont("Montserrat-Regular.ttf", "Regular");
					fonts.AddFont("Montserrat-SemiBold.ttf", "SemiBold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}

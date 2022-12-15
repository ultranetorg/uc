using CommunityToolkit.Maui;
using Sharpnado.Tabs;

namespace UC.Umc;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureEssentials()
			.ConfigureServices()
			.ConfigureViewModels()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Montserrat-Bold.ttf", "Bold");
				fonts.AddFont("Montserrat-SemiBold.ttf", "SemiBold");
				fonts.AddFont("Montserrat-Light.ttf", "Light");
				fonts.AddFont("Montserrat-Italic.ttf", "Italic");
				fonts.AddFont("Montserrat-Medium.ttf", "Medium");
				fonts.AddFont("Montserrat-Regular.ttf", "Regular");
			})
			.UseSharpnadoTabs(false, true);

#if DEBUG
		builder.Services.AddLogging(configure =>
			configure.AddDebug().SetMinimumLevel(LogLevel.Debug)
		);
#endif

		return builder.Build();
	}
}

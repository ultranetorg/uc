﻿using CommunityToolkit.Maui;

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
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
        builder.Services.AddLogging(configure =>
            configure.AddDebug().SetMinimumLevel(LogLevel.Debug)
        );
#endif

		return builder.Build();
	}
}
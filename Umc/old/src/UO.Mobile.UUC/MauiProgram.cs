using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace UO.Mobile.UUC;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Montserrat-Bold", "MontserratBold");
                fonts.AddFont("Montserrat-Regular", "MontserratRegular");
                fonts.AddFont("Montserrat-SemiBold", "MontserratSemiBold");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}

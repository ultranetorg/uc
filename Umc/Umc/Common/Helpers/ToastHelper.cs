using CommunityToolkit.Maui.Core;

namespace UC.Umc.Helpers;

/// <summary>
/// Shows a timed alert at the bottom of the screen
/// </summary>
internal static class ToastHelper
{
    public static Task ShowMessageAsync(string message,
        ToastDuration duration = ToastDuration.Long,
        double fontSize = 14,
        string title = "Alert Message")
    {
#if ANDROID
        var toast = CommunityToolkit.Maui.Alerts.Toast.Make(message, duration, fontSize);
        return toast.Show(new CancellationTokenSource().Token);
#elif IOS
        // Toast currently errors on iOS
		return App.Current.MainPage.DisplayAlert(title, message, "OK");
#endif
    }

    public static Task ShowDefaultErrorMessageAsync(ToastDuration duration = ToastDuration.Short,
        double fontSize = 14) => ShowMessageAsync("Something went wrong", duration, fontSize, "Unknown Error");


    public static void ShowErrorMessage(ILogger logger, string message = "Something Went Wrong",
        ToastDuration duration = ToastDuration.Short, double fontSize = 14, string title = "Error") =>
        ShowMessageAsync(message, duration, fontSize, title)
            .AwaitTaskAsync(e => logger.LogError(e, "ToastHelper Exception: {Ex}", e.Message), false);
}

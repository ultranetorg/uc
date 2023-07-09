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
        string title = null)
    {
#if ANDROID
        var toast = CommunityToolkit.Maui.Alerts.Toast.Make(message, duration, fontSize);
        return toast.Show(new CancellationTokenSource().Token);
#elif IOS
        // Toast currently errors on iOS
		return App.Current.MainPage.DisplayAlert(
			title ?? Properties.Additional_Strings.Alert_Message,
			message, Properties.Additional_Strings.Alert_Ok);
#endif
    }

    public static Task ShowDefaultErrorMessageAsync(ToastDuration duration = ToastDuration.Short,
        double fontSize = 14) => ShowMessageAsync(Properties.Additional_Strings.Error_Default,
			duration, fontSize, Properties.Additional_Strings.Error_DefaultTitle);


    public static void ShowErrorMessage(ILogger logger, string message = null,
        ToastDuration duration = ToastDuration.Short, double fontSize = 14, string title = null) =>
        ShowMessageAsync(message ?? Properties.Additional_Strings.Error_Default,
			duration, fontSize, title ?? Properties.Additional_Strings.Error_DefaultTitle)
            .AwaitTaskAsync(e => logger.LogError(e, "ToastHelper Exception: {Ex}", e.Message), false);
}

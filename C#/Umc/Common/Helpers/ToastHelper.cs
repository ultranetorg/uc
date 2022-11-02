using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace UC.Umc.Helpers;

/// <summary>
/// Shows a timed alert at the bottom of the screen
/// </summary>
internal static class ToastHelper
{
    public static async Task ShowMessageAsync(string text, ToastDuration duration = ToastDuration.Long,
        double fontSize = 14)
    {
#if ANDROID
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(new CancellationTokenSource().Token);
#elif IOS
        // Toast currently errors on iOS
		await App.Current.MainPage.DisplayAlert(text, string.Empty, "Ok");
#endif
    }
    public static async Task ShowDefaultErrorMessageAsync(ToastDuration duration = ToastDuration.Short,
        double fontSize = 14)
    {
        await ShowMessageAsync("Something went wrong", duration, fontSize);
    }
}

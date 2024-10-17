using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

namespace UC.Umc.Common.Helpers;

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
#if IOS
		// Toast currently errors on iOS
		return App.Current.MainPage.DisplayAlert(title, message, "OK");
#else
		var toast = Toast.Make(message, duration, fontSize);
		return toast.Show(new CancellationTokenSource().Token);
#endif
	}

	public static Task ShowDefaultErrorMessageAsync(ToastDuration duration = ToastDuration.Short,
		double fontSize = 14) => ShowMessageAsync("Something went wrong", duration, fontSize, "Unknown Error");


	public static void ShowErrorMessage(ILogger logger, string message = "Something Went Wrong",
		ToastDuration duration = ToastDuration.Short, double fontSize = 14, string title = "Error") =>
		ShowMessageAsync(message, duration, fontSize, title)
			.AwaitTaskAsync(e => logger.LogError(e, "ToastHelper Exception: {Ex}", e.Message), false);
}

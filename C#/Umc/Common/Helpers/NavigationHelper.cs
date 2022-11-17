namespace UC.Umc.Helpers;

public static class Navigation
{
    public static string CurrentLocation => Shell.Current.CurrentState.Location.OriginalString;

    public static Task GoToAsync(ShellNavigationState state, IDictionary<string, object> parameters = null) =>
        MainThread.InvokeOnMainThreadAsync(() => (parameters != null)
            ? Shell.Current.GoToAsync(state, parameters)
            : Shell.Current.GoToAsync(state));

    public static Task OpenModalAsync<TPage>() where TPage : Page =>
        MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.Navigation.PushModalAsync(Ioc.Default.GetService<TPage>()));

    public static Task PopAsync() => MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(".."));

    public static Task PopModalAsync(bool isAnimated = true) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.Navigation.PopModalAsync(isAnimated));

    internal static async Task GoToUpwardsAsync(string route) => await GoToAsync($"//{route}");
}

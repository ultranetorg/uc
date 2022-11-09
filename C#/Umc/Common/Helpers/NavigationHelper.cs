namespace UC.Umc.Helpers;

public static class Navigation
{
    internal static async Task GoToAsync(ShellNavigationState state,
		IDictionary<string, object> parameters = null)
    {
        if (parameters != null)
        {
            await Shell.Current.GoToAsync(state, parameters);
        }
        else
        {
            await Shell.Current.GoToAsync(state);
        }
    }

	internal static async Task PopModalAsync() => await Shell.Current.Navigation.PopModalAsync();

    internal static async Task GoToUpwardsAsync(string route) => await GoToAsync($"//{route}");
}

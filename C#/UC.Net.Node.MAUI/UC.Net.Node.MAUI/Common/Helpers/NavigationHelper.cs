namespace UC.Net.Node.MAUI.Helpers;

public static class Navigation
{
    internal static async Task NavigateToAsync(ShellNavigationState state,
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

	
    internal static async Task GoToUpwardsAsync(string route) => await NavigateToAsync($"//{route}");
}

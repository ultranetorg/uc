namespace UC.Umc.Helpers;

public static class ConnectivityHelper
{
    public static async Task<bool> Check()
    {
		var access = Connectivity.NetworkAccess == NetworkAccess.Internet;
        if (!access)
        {
			await App.Current.MainPage.ShowPopupAsync(new AlertPopup("LostConnection"));
        }
		return access;
    }

    static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        var access = e.NetworkAccess;
        var profiles = e.ConnectionProfiles;
    }
}

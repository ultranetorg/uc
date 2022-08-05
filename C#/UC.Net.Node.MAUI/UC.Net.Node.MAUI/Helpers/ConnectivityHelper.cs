namespace UC.Net.Node.MAUI.Helpers;

public static class ConnectivityHelper
{
    public static async Task<bool> Check()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            return true;
        }
        else
        {
            await App.Current.MainPage.ShowPopupAsync(new AlertPopup("LostConnection"));
        }
        return false;
    }

    static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        var access = e.NetworkAccess;
        var profiles = e.ConnectionProfiles;
    }
}

using UC.Net.Node.MAUI.Popups;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace UC.Net.Node.MAUI.Helpers
{
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
               await AlertPopup.Show("LostConnection");
            }
            return false;
        }

        static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var access = e.NetworkAccess;
            var profiles = e.ConnectionProfiles;
        }
    }
}

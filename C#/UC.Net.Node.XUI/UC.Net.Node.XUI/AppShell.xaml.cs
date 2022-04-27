using Xamarin.Forms;

namespace UO.Mobile.Wallet
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			HideTabAndNavBar();
		}

		private void HideTabAndNavBar()
		{
			SetNavBarIsVisible(this, false);
			SetTabBarIsVisible(this, false);
		}
	}
}
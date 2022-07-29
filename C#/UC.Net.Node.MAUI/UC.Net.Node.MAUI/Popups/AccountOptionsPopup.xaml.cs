using UC.Net.Node.MAUI.Pages;

namespace UC.Net.Node.MAUI.Popups
{
    public partial class AccountOptionsPopup : Popup
    {
        public AccountOptionsPopup()
        {
            InitializeComponent();
            // Wallet = wallet;
            BindingContext = this;
        }

		[RelayCommand]
		private async void Send()
		{
			await Shell.Current.Navigation.PushAsync(new SendPage());
		}

		public void Hide()
		{
			Close();
		}
		
        // public Wallet Wallet { get; }
		//public static async Task Show(Wallet wallet)
		//{
		//	popup = new AccountOptionsPopup(wallet);
		//	await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
		//}
	}
}
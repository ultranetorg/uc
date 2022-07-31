namespace UC.Net.Node.MAUI.Popups
{
    public partial class DeleteAccountPopup : Popup
    {
        private static DeleteAccountPopup popup;
		public Wallet Wallet { get; }

        public DeleteAccountPopup(Wallet wallet)
        {
            InitializeComponent();
			Wallet = wallet;
			BindingContext = this;
        }

        public void Hide()
        {
			Close();
        }

		public static async Task Show(Wallet wallet)
		{
			popup = new DeleteAccountPopup(wallet);
			await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
		}
	}    
}
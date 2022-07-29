namespace UC.Net.Node.MAUI.Popups
{
    public partial class DeleteAccountPopup : Popup
    {
        public DeleteAccountPopup()
        {
            InitializeComponent();
            //Wallet = wallet;
            BindingContext = this;
        }

        public void Hide()
        {
			Close();
        }
		
        //public Wallet Wallet { get; }
        //public static async Task Show(Wallet wallet)
        //{
        //    popup = new DeleteAccountPopup(wallet);
        //    await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
        //}
    }    
}
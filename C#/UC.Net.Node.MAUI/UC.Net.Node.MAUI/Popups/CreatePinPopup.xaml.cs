namespace UC.Net.Node.MAUI.Popups
{
    public partial class CreatePinPopup : Popup
    {
        public CreatePinPopup( )
        {
            InitializeComponent();
            BindingContext = this;
        }

        public void Hide()
        {
			Close();
        }

        //public static async Task Show()
        //{
        //    popup = new CreatePinPopup();
        //    await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
        //}
    }
}
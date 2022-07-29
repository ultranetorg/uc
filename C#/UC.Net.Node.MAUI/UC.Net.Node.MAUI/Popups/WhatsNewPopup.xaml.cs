namespace UC.Net.Node.MAUI.Popups
{
    public partial class WhatsNewPopup : Popup
    {
        public WhatsNewPopup()
        {
            InitializeComponent();
        }
       
        public void Hide()
        {
			Close();
        }

        //public static async Task Show()
        //{
        //    popup = new WhatsNewPopup();
        //    await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
        //}
    }
}
namespace UC.Net.Node.MAUI.Popups;

public partial class NotificationPopup : Popup
{
    public NotificationPopup()
    {
        InitializeComponent();
        BindingContext = new NotificationViewModel(ServiceHelper.GetService<ILogger<NotificationViewModel>>());
    }

    public void Hide()
    {
        Close();
    }

    //public static async Task Show()
    //{
    //    popup = new NotificationPopup();
    //    await App.Current.MainPage.Navigation.ShowPopupAsDialog(popup);
    //}
}
namespace UC.Net.Node.MAUI.Popups;

public partial class NotificationsPopup : Popup
{
    private static NotificationsPopup popup;

    public NotificationsPopup()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<NotificationsViewModel>();;
    }
       
    public void Hide()
    {
		Close();
    }

	public static async Task Show()
	{
		popup = new NotificationsPopup();
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
}
namespace UC.Net.Node.MAUI.Popups;

public partial class NotificationPopup : Popup
{
    private static NotificationPopup popup;

    public NotificationPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<NotificationViewModel>();
    }

    public void Hide()
    {
        Close();
    }

	public static async Task Show()
	{
		popup = new NotificationPopup();
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
}
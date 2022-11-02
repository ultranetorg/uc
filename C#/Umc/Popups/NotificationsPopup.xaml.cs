namespace UC.Umc.Popups;

public partial class NotificationsPopup : Popup
{
    private static NotificationsPopup popup;

    public NotificationsPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<NotificationsViewModel>();;
    }
       
    public void Hide()
    {
		Close();
    }

	// TBR async
	public static async Task Show()
	{
		popup = new NotificationsPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}
namespace UC.Net.Node.MAUI.Popups;

public partial class RecipientAccountPopup : Popup
{
	private static RecipientAccountPopup popup;

    public RecipientAccountPopup()
    {
        InitializeComponent();
        BindingContext = new RecipientAccountViewModel(this, ServiceHelper.GetService<ILogger<NotificationsViewModel>>());
    }

    public void Hide()
    {
		Close();
    }

	public static async Task Show()
	{
		popup = new RecipientAccountPopup();
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
}
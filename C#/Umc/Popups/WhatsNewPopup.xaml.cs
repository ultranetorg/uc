namespace UC.Umc.Popups;

public partial class WhatsNewPopup : Popup
{
    private static WhatsNewPopup popup;

    public WhatsNewPopup()
    {
        InitializeComponent();
    }
       
    public void Hide()
    {
	Close();
    }

	public static async Task Show()
	{
		popup = new WhatsNewPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}
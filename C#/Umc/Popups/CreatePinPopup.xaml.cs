namespace UC.Umc.Popups;

public partial class CreatePinPopup : Popup
{
    private static CreatePinPopup popup;

    public CreatePinPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task Show()
	{
		popup = new CreatePinPopup();
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}
namespace UC.Net.Node.MAUI.Popups;

public partial class DeleteAccountPopup : Popup
{
    private static DeleteAccountPopup popup;
	public Account Account { get; }

    public DeleteAccountPopup(Account account)
    {
        InitializeComponent();
		Account = account;
		BindingContext = this;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task Show(Account account)
	{
		popup = new DeleteAccountPopup(account);
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
} 
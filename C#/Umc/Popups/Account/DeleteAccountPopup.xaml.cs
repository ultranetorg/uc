namespace UC.Umc.Popups;

public partial class DeleteAccountPopup : Popup
{
    private static DeleteAccountPopup popup;
	public AccountViewModel Account { get; }

    public DeleteAccountPopup(AccountViewModel account)
    {
        InitializeComponent();
		Account = account;
		BindingContext = this;
    }

    public void Hide()
    {
		Close();
    }

	public static async Task Show(AccountViewModel account)
	{
		popup = new DeleteAccountPopup(account);
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
} 
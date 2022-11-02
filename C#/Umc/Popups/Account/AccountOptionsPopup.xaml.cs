namespace UC.Umc.Popups;

public partial class AccountOptionsPopup : Popup
{
	private static AccountOptionsPopup popup;
	
    public AccountViewModel Account { get; }

    public AccountOptionsPopup(AccountViewModel account)
    {
        InitializeComponent();
        Account = account;
        BindingContext = this;
    }

	[RelayCommand]
	private async Task Send()
	{
		await Shell.Current.Navigation.PushAsync(new SendPage());
	}

	public void Hide()
	{
		Close();
	}

	public static async Task Show(AccountViewModel account)
	{
		popup = new AccountOptionsPopup(account);
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}
}
namespace UC.Net.Node.MAUI.Popups;

public partial class AccountOptionsPopup : Popup
{
	private static AccountOptionsPopup popup;
	
    public Account Account { get; }

    public AccountOptionsPopup(Account account)
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

	public static async Task Show(Account account)
	{
		popup = new AccountOptionsPopup(account);
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
}
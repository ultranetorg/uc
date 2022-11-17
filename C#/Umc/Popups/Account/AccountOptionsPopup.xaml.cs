namespace UC.Umc.Popups;

public partial class AccountOptionsPopup : Popup
{
	private static AccountOptionsPopup popup;

	AccountOptionsViewModel Vm => BindingContext as AccountOptionsViewModel;

    public AccountOptionsPopup(AccountViewModel account)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AccountOptionsViewModel>();
		Vm.Account = account;
    }

	public static async Task Show(AccountViewModel account)
	{
		popup = new AccountOptionsPopup(account);
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}

	public void Hide()
	{
		Close();
	}
}
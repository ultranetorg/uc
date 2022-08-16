namespace UC.Net.Node.MAUI.Popups;

public partial class AccountOptionsPopup : Popup
{
private static AccountOptionsPopup popup;
	
    public Wallet Wallet { get; }

    public AccountOptionsPopup(Wallet wallet)
    {
        InitializeComponent();
        Wallet = wallet;
        BindingContext = this;
    }

	[RelayCommand]
	private async void Send()
	{
		await Shell.Current.Navigation.PushAsync(new SendPage());
	}

	public void Hide()
	{
		Close();
	}

	// this will be static helper method
	public static async Task Show(Wallet wallet)
	{
		popup = new AccountOptionsPopup(wallet);
		await App.Current.MainPage.ShowPopupAsync(popup);
	}
}
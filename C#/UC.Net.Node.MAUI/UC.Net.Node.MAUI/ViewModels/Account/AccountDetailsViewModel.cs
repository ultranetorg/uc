namespace UC.Net.Node.MAUI.ViewModels.Account;

public partial class AccountDetailsViewModel : BaseAccountViewModel
{
	// todo: split into 3 services
	private readonly IServicesMockData _service;

    public AccountDetailsViewModel(IServicesMockData service, ILogger<AccountDetailsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private async Task SendAsync()
    {
        await Shell.Current.Navigation.PushAsync(new SendPage());
    }

	[RelayCommand]
    private async Task ShowKeyAsync()
    {
        await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Wallet));
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Wallet));
    }

	[RelayCommand]
    private void SelectRandomColor() => Wallet.AccountColor = ColorHelper.GetRandomColor();

	private void LoadData()
	{
		Authors.Clear();
		Products.Clear();
		ColorsCollection.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
		ColorsCollection.AddRange(_service.AccountColors);

		// will be replaced from query parameter
		AccountName = "Account Name";
		
		// TODO: add workflow object, the wallet is coming from api
	}
}

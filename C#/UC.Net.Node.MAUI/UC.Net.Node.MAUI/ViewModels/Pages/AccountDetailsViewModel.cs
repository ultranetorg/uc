namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AccountDetailsViewModel : BaseAccountViewModel
{
	// will be splitted into 3 services
	private readonly IServicesMockData _service;

    public AccountDetailsViewModel(IServicesMockData service, ILogger<AccountDetailsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private async void SendAsync()
    {
        await Shell.Current.Navigation.PushAsync(new SendPage());
    }

	[RelayCommand]
    private async void ShowKeyAsync()
    {
        await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Wallet));
    }

	[RelayCommand]
    private async void DeleteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Wallet));
    }

	private void LoadData()
	{
		// TBR
		Authors.Clear();
		Products.Clear();
		ColorsCollection.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
		ColorsCollection.AddRange(_service.AccountColors);
	}
}

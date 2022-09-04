namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AccountDetailsViewModel : BaseAccountViewModel
{
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
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#6601e3") ,BoderColor = Shell.Current.BackgroundColor });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"),  BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#d56a48"),  BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = Colors.Transparent });

		Authors.Clear();
		Products.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
	}
}

namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class NetworkViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Emission> _emissions = new();
    
	[ObservableProperty]
    private Wallet _wallet;

    public NetworkViewModel(ILogger<NetworkViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	[RelayCommand]
    private async void TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	private void FillFakeData()
	{
		_wallet = new()
		{
			Id = Guid.NewGuid(),
			Unts = 5005,
			IconCode = "47F0",
			Name = "Main ultranet wallet",
			AccountColor = Color.FromArgb("#6601e3"),
		}; 
        Emissions.Add(new Emission { ETH = "100", Number = 1,UNT = "100" });
        Emissions.Add(new Emission { ETH = "1000", Number = 2, UNT = "1000" });
        Emissions.Add(new Emission { ETH = "10000", Number = 3, UNT = "10000" });
        Emissions.Add(new Emission { ETH = "100000", Number = 4, UNT = "10000" });
	}
}

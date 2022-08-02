namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class UnfinishTransferViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Emission> _emissions;

	[ObservableProperty]
    private Wallet _wallet;

    public UnfinishTransferViewModel(ILogger<UnfinishTransferViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    private async void CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
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
		_emissions = new();
		Emissions.Add(new Emission { ETH = "100", Number=1,UNT="100" });
        Emissions.Add(new Emission { ETH = "1000", Number = 2, UNT = "1000" });
        Emissions.Add(new Emission { ETH = "10000", Number = 3, UNT = "10000" });
        Emissions.Add(new Emission { ETH = "100000", Number = 4, UNT = "10000" });
	}
}

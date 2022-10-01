namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class NetworkViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Emission> _emissions = new();
    
	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

    public NetworkViewModel(IServicesMockData service, ILogger<NetworkViewModel> logger) : base(logger)
    {
		_service = service;
		Initialize();
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	private void Initialize()
	{
		Emissions.Clear();
		Emissions.AddRange(_service.Emissions);
	}

	// TODO: Add RegisterProductCommand
}

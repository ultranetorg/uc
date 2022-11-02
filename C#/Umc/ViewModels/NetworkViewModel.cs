namespace UC.Umc.ViewModels;

public partial class NetworkViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Models.Emission> _emissions = new();
    
	[ObservableProperty]
    private AccountViewModel _account;

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
		Account = DefaultDataMock.CreateAccount();

		Emissions.Clear();
		Emissions.AddRange(_service.Emissions);
	}

	// TODO: Add RegisterProductCommand
}

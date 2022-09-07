namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class NetworkViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Emission> _emissions = new();
    
	[ObservableProperty]
    private Wallet _wallet = new()
	{
		Id = Guid.NewGuid(),
		Unts = 5005,
		IconCode = "47F0",
		Name = "Main ultranet wallet",
		AccountColor = Color.FromArgb("#6601e3"),
	};

    public NetworkViewModel(IServicesMockData service, ILogger<NetworkViewModel> logger) : base(logger)
    {
		_service = service;
		Initialize();
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

	private void Initialize()
	{
		Emissions.Clear();
		Emissions.AddRange(_service.Emissions);
	}
}

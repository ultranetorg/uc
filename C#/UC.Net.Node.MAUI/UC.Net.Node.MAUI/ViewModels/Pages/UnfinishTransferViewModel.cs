namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class UnfinishTransferViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Emission> _emissions = new();

	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

    public UnfinishTransferViewModel(IServicesMockData service, ILogger<UnfinishTransferViewModel> logger) : base(logger)
    {
		_service = service;
		Initialize();
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

	private void Initialize()
	{
		Emissions.Clear();
		Emissions.AddRange(_service.Emissions);
	}
}

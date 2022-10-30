namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class UnfinishTransferViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Models.Emission> _emissions = new();

	[ObservableProperty]
    private AccountViewModel _account;

    public UnfinishTransferViewModel(IServicesMockData service, ILogger<UnfinishTransferViewModel> logger) : base(logger)
    {
		_service = service;
		Initialize();
    }

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	private void Initialize()
	{
		Emissions.Clear();
		Emissions.AddRange(_service.Emissions);
		Account = DefaultDataMock.CreateAccount();
	}
}

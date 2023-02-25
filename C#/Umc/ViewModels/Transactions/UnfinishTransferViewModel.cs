namespace UC.Umc.ViewModels;

public partial class UnfinishTransferViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private CustomCollection<Emission> _emissions = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(UntAmount))]
	private decimal _ethAmount = 112;

	public decimal UntAmount => EthAmount * 10;

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

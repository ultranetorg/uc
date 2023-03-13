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
	private decimal _ethAmount;

	public decimal UntAmount => EthAmount * 10;

    public UnfinishTransferViewModel(IServicesMockData service, ILogger<UnfinishTransferViewModel> logger) : base(logger)
    {
		_service = service;
		Initialize();
    }

	[RelayCommand]
    private async Task TransferAsync()
    {
        await Navigation.GoToUpwardsAsync(Routes.TRANSFER, new Dictionary<string, object>()
		{
			{QueryKeys.ACCOUNT, Account},
			{QueryKeys.ETH_AMOUNT, EthAmount}
		});
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        await Navigation.PopAsync();
    }

	private void Initialize()
	{
		Emissions.Clear();
		Emissions.AddRange(_service.Emissions);
		Account = DefaultDataMock.CreateAccount();
		EthAmount = new Random().Next(1, 100);
	}
}

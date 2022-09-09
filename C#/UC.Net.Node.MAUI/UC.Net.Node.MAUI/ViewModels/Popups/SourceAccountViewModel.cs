namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class SourceAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

    public SourceAccountPopup Popup { get; set; }

	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

    public SourceAccountViewModel(IServicesMockData service, ILogger<SourceAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void ItemTapped(Wallet wallet)
    {
        foreach (var item in Wallets)
        {
            item.IsSelected = false;
        }
        wallet.IsSelected = true;
    }

	[RelayCommand]
    private void Close()
    {
        Popup.Hide();
    }
	
	private void LoadData()
	{
		Wallets.Clear();
		Wallets.AddRange(_service.Wallets);
	}
}

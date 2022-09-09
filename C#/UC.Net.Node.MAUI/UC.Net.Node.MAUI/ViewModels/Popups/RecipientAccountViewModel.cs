namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class RecipientAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

    public RecipientAccountPopup Popup { get; set;}

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();
        
	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

    public RecipientAccountViewModel(IServicesMockData service, ILogger<RecipientAccountViewModel> logger): base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void Close()
    {
        Popup.Hide();
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
	
	private void LoadData()
	{
		Wallets.Clear();
		Wallets.AddRange(_service.Wallets);
	}
}

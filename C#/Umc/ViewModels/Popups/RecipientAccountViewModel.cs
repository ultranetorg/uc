namespace UC.Umc.ViewModels.Popups;

public partial class RecipientAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();
        
	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
	private bool _isLocal = true;

	[ObservableProperty]
	private bool _isExternal;

	[ObservableProperty]
	private bool _isQrCode;

    public RecipientAccountViewModel(IServicesMockData service, ILogger<RecipientAccountViewModel> logger): base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void Close() => ClosePopup();

	[RelayCommand]
    private void ItemTapped(AccountViewModel account)
    {
        //foreach (var item in Accounts)
        //{
        //    item.IsSelected = false;
        //}
        //wallet.IsSelected = true;
    }
	
	private void LoadData()
	{
		Accounts.Clear();
		Accounts.AddRange(_service.Accounts);
		Account = DefaultDataMock.CreateAccount();
	}
}

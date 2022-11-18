namespace UC.Umc.ViewModels.Popups;

public partial class SourceAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

    public SourceAccountViewModel(IServicesMockData service, ILogger<SourceAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void ItemTapped(AccountViewModel account)
    {
        //foreach (var item in Accounts)
        //{
        //    item.IsSelected = false;
        //}
        //wallet.IsSelected = true;
    }

	[RelayCommand]
    private void Close() => ClosePopup();
	
	private void LoadData()
	{
		Accounts.Clear();
		Accounts.AddRange(_service.Accounts);

		Account = DefaultDataMock.CreateAccount();
	}
}

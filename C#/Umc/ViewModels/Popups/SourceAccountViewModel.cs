namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class SourceAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

    public SourceAccountPopup Popup { get; set; }

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
    private void Close()
    {
        Popup.Hide();
    }
	
	private void LoadData()
	{
		Accounts.Clear();
		Accounts.AddRange(_service.Accounts);

		Account = DefaultDataMock.CreateAccount();
	}
}

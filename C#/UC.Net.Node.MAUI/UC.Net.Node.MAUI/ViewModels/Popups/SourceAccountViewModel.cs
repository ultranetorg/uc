namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class SourceAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

    public SourceAccountPopup Popup { get; set; }

	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

	[ObservableProperty]
    private CustomCollection<Account> _accounts = new();

    public SourceAccountViewModel(IServicesMockData service, ILogger<SourceAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void ItemTapped(Account account)
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
	}
}

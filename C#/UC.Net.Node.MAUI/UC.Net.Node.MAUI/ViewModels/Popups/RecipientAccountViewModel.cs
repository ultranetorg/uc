namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class RecipientAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

    public RecipientAccountPopup Popup { get; set;}

	[ObservableProperty]
    private CustomCollection<Account> _accounts = new();
        
	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

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
	}
}

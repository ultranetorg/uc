namespace UC.Net.Node.MAUI.ViewModels.Account;

public partial class CreateAccountPageViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public CreateAccountPageViewModel(IServicesMockData service, ILogger<CreateAccountPageViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	private void LoadData()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);
	}
}
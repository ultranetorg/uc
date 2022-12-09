namespace UC.Umc.ViewModels;

public partial class CreateAccountPageViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public CreateAccountPageViewModel(IServicesMockData service, ILogger<CreateAccountPageViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);

		await Task.Delay(1);
	}
}
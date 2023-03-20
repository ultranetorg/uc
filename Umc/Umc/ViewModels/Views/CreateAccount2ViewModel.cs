namespace UC.Umc.ViewModels.Views;

public partial class CreateAccount2ViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public CreateAccount2ViewModel(INotificationsService notificationService, IServicesMockData service,
		ILogger<CreateAccount2ViewModel> logger) : base(notificationService,logger)
    {
		_service = service;
		LoadData();
	}

	[RelayCommand]
	private void ColorTapped(AccountColor accountColor)
	{
		foreach (var item in ColorsCollection)
		{
			item.BorderColor = Colors.Transparent;
		}
		accountColor.BorderColor = Shell.Current.BackgroundColor;
		Account.Color = ColorHelper.CreateGradientColor(accountColor.Color);
		SelectedAccountColor = accountColor;
	}

	[RelayCommand]
    private void Randomize()
    {
        ColorTapped(DefaultDataMock.CreateRandomColor());
    }

	private void LoadData()
	{
		Account = DefaultDataMock.CreateAccount("Test");
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);
        SelectedAccountColor = ColorsCollection.First();
	}
}

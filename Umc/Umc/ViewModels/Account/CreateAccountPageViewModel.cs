namespace UC.Umc.ViewModels;

public partial class CreateAccountPageViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountColor _selectedAccountColor;

    public CreateAccountPageViewModel(INotificationsService notificationService, IServicesMockData service,
		ILogger<CreateAccountPageViewModel> logger) : base(notificationService, logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);

		await Task.Delay(1);
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		if (Position == 0)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully created!");
		}
	}
}
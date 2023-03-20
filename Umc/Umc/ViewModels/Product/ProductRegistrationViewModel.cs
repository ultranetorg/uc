namespace UC.Umc.ViewModels;

public partial class ProductRegistrationViewModel : BasePageViewModel
{
	[ObservableProperty]
	private string _name = string.Empty;

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	[ObservableProperty]
	private AccountViewModel _account;

	[ObservableProperty]
    private int _position;

    public ProductRegistrationViewModel(INotificationsService notificationService, ILogger<ProductRegistrationViewModel> logger) : base(notificationService, logger)
    {
    }

    [RelayCommand]
    private async Task SelectAccountAsync()
	{
		try
		{
			var popup = new SourceAccountPopup();
			await ShowPopup(popup);
			if (popup.Vm?.Account != null)
			{
				Account = popup.Vm.Account;
			}
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, "SelectAccountAsync Exception: {Ex}", ex.Message);
		}
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
			await ToastHelper.ShowMessageAsync("Successfully registered!");
		}
	}

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}
}

namespace UC.Umc.ViewModels;

public partial class AuthorRenewalViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(EstimatedDate))]
	private string _period = "10"; // TODO: replace with time span

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	public string EstimatedDate => string.IsNullOrWhiteSpace(Period)
		? string.Empty
		: DateTime.UtcNow.AddDays(int.Parse(Period)).ToShortDateString();

    public AuthorRenewalViewModel(INotificationsService notificationService,
		ILogger<AuthorRenewalViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
	private async Task NextWorkaroundNewAsync()
	{
		var isValid = Account != null && !string.IsNullOrEmpty(Period);
		if (Position == 0 && isValid)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else if (Position == 1)
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Success!");
		}
	}

	[RelayCommand]
	protected async Task CancelAsync()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
		else
		{
			await Navigation.PopAsync();
		}
	}
}
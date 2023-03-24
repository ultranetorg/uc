using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class AccountDetailsViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [NotifyPropertyChangedFor(nameof(AccountNameError))]
    private string _accountName;

    public string AccountNameError => GetControlErrorMessage(nameof(AccountName));

	[ObservableProperty]
    public GradientBrush _background;

    public AccountDetailsViewModel(INotificationsService notificationService, IServicesMockData service,
		ILogger<AccountDetailsViewModel> logger) : base(notificationService, logger)
    {
		_service = service;
		LoadData();
    }

	// !TO DO: set background in model only in VM after submitting

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Account = (AccountViewModel)query[QueryKeys.ACCOUNT];
			AccountName = Account.Name;
			Background = Account.Color;
#if DEBUG
            _logger.LogDebug("ApplyQueryAttributes Account: {Account}", Account);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApplyQueryAttributes Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
        finally
        {
            FinishLoading();
        }
	}

	[RelayCommand]
    private void SetAccountColor(AccountColor accountColor) =>
		Background = accountColor != null 
			? ColorHelper.CreateGradientColor(accountColor.Color)
			: ColorHelper.CreateRandomGradientColor();

	[RelayCommand]
    private async Task SendAsync() =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, Account },
			{ QueryKeys.RECIPIENT_ACCOUNT, null }
		});

	[RelayCommand]
    private async Task ReceiveAsync() =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, null },
			{ QueryKeys.RECIPIENT_ACCOUNT, Account }
		});

	[RelayCommand]
    private async Task HideFromDashboardAsync()
    {
		// TODO
		await Task.Delay(1);
    }

	[RelayCommand]
    private async Task ShowPrivateKeyAsync() =>
		await Navigation.GoToAsync(nameof(PrivateKeyPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.ACCOUNT, Account }
		});

	[RelayCommand]
    private async Task BackupAsync()
    {
		// TODO
		await Task.Delay(1);
    }

	[RelayCommand]
    private async Task DeleteAsync() =>
		await Navigation.GoToAsync(nameof(DeleteAccountPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.ACCOUNT, Account }
		});

	private void LoadData()
	{
		Authors.Clear();
		Products.Clear();
		ColorsCollection.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
		ColorsCollection.AddRange(_service.AccountColors);
	}
}

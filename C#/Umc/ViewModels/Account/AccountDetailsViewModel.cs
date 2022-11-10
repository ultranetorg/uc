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

    public AccountDetailsViewModel(IServicesMockData service, ILogger<AccountDetailsViewModel> logger) : base(logger)
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

            Account = (AccountViewModel)query[nameof(AccountDetailsPage)];
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
    private async Task SendAsync()
    {
        await Shell.Current.Navigation.PushAsync(new SendPage());
    }

	[RelayCommand]
    private async Task ShowKeyAsync()
    {
        await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Account));
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Account));
    }

	[RelayCommand]
    private void SetAccountColor(AccountColor accountColor) =>
		Background = accountColor != null 
			? ColorHelper.CreateGradientColor(accountColor.Color)
			: ColorHelper.CreateRandomGradientColor();

	private void LoadData()
	{
		Authors.Clear();
		Products.Clear();
		ColorsCollection.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
		ColorsCollection.AddRange(_service.AccountColors);
		
		// TODO: add workflow object, the wallet is coming from api
	}
}

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
    public LinearGradientBrush _background;

    public AccountDetailsViewModel(IServicesMockData service, ILogger<AccountDetailsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	// !NEXT STEPS TO DO:
	// 1. retrieve account object from query parameter after redirecting from ManageAccountsPage
	// 2. set background in model only in VM after submitting

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
    private void SelectRandomColor() => Account.Color = ColorHelper.CreateRandomGradientColor();

	private void LoadData()
	{
		Authors.Clear();
		Products.Clear();
		ColorsCollection.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
		ColorsCollection.AddRange(_service.AccountColors);

		// will be replaced from query parameter
		AccountName = "Account Name";
		
		// TODO: add workflow object, the wallet is coming from api
	}
}

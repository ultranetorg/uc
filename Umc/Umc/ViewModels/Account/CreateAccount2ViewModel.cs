using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class CreateAccount2ViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorCode))]
    private AccountColor _selectedAccountColor;

	[ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [NotifyPropertyChangedFor(nameof(AccountNameError))]
    private string _accountName;

	public string ColorCode => SelectedAccountColor?.Color?.ToHex() ?? "#ABCD";

    public string AccountNameError => GetControlErrorMessage(nameof(AccountName));

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
		Account = DefaultDataMock.CreateAccount("Test Account");
		ColorsCollection = new CustomCollection<AccountColor>(_service.AccountColors);
        SelectedAccountColor = ColorsCollection.First();
	}
}

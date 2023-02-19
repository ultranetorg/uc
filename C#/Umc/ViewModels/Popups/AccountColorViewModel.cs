namespace UC.Umc.ViewModels.Popups;

public partial class AccountColorViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    public GradientBrush _background;

	[ObservableProperty]
    private CustomCollection<AccountColor> _colorsCollection = new();

	public AccountColorViewModel(IServicesMockData service, ILogger<AccountColorViewModel> logger) : base(logger)
	{
		_service = service;
		LoadData();
	}

	[RelayCommand]
    private void SetAccountColor(AccountColor accountColor) =>
		Background = accountColor != null 
			? ColorHelper.CreateGradientColor(accountColor.Color)
			: ColorHelper.CreateRandomGradientColor();

	[RelayCommand]
    private void SubmitColor() => ClosePopup();

	private void LoadData()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);
	}
}

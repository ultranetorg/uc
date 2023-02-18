using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class RestoreAccountViewModel : BaseAccountViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
	private bool _isPrivateKey;

	[ObservableProperty]
	private bool _isFilePath = true;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletFilePath;

	[ObservableProperty]
    public GradientBrush _background;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [NotifyPropertyChangedFor(nameof(AccountNameError))]
    private string _accountName;

    public string AccountNameError => GetControlErrorMessage(nameof(AccountName));

    public RestoreAccountViewModel(IServicesMockData service, ILogger<RestoreAccountViewModel> logger) : base(logger)
    {
		_service = service;
    }

	[RelayCommand]
	private void ChangeKeySource()
	{
		IsPrivateKey = !IsPrivateKey;
		IsFilePath = !IsFilePath;
	}

	[RelayCommand]
    private async Task ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
	}

	[RelayCommand]
    private void SetAccountColor(AccountColor accountColor) =>
		Background = accountColor != null 
			? ColorHelper.CreateGradientColor(accountColor.Color)
			: ColorHelper.CreateRandomGradientColor();

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
		else if (Position == 1)
		{
			Position = 2;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully created!");
		}
	}

	internal async Task InitializeAsync()
	{
		ColorsCollection.Clear();
		ColorsCollection.AddRange(_service.AccountColors);

		await Task.Delay(1);
	}
}

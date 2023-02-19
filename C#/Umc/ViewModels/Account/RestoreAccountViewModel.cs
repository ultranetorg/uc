using System.ComponentModel.DataAnnotations;

namespace UC.Umc.ViewModels;

public partial class RestoreAccountViewModel : BaseAccountViewModel
{
	[ObservableProperty]
	private bool _isPrivateKey;

	[ObservableProperty]
	private bool _isFilePath = true;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletFilePath;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [NotifyPropertyChangedFor(nameof(AccountNameError))]
    private string _accountName;

    public string AccountNameError => GetControlErrorMessage(nameof(AccountName));

	[ObservableProperty]
    public GradientBrush _background;

    public RestoreAccountViewModel(ILogger<RestoreAccountViewModel> logger) : base(logger)
    {
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
    private async Task OpenAccountColorPopupAsync()
	{
		try
		{
			var popup = new AccountColorPopup();
			await ShowPopup(popup);
			
			if (popup.Vm?.Background != null)
			{
				Background = popup.Vm.Background;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenAccountColorPopupAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		try
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
		catch (Exception ex)
		{
			_logger.LogError("NextWorkaroundAsync Error: {Message}", ex.Message);
		}
		
	}
}

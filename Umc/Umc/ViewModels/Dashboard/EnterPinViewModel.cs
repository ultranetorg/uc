namespace UC.Umc.ViewModels;

public partial class EnterPinViewModel : BasePageViewModel
{
	private AuthService _authService;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ShowBiometric))]
	[NotifyPropertyChangedFor(nameof(ShowLogin))]
	[NotifyPropertyChangedFor(nameof(Logo))]
	private string _pincode = string.Empty;

	public string Logo => $"logo{Pincode.Length}_dark.png";
	public bool ShowBiometric => !(Pincode.Length > 0);
    public bool ShowLogin => Pincode.Length > 0;

    public EnterPinViewModel(AuthService authService, INotificationsService notificationService, ILogger<EnterPinViewModel> logger) : base(notificationService, logger)
    {
		_authService = authService;
    }

	// If pin not set opens CreatePinPage
	public async Task InitializeAsync()
	{
		try
		{
			// DEBUG
			// await Navigation.GoToUpwardsAsync(nameof(DashboardPage));

			var pin = await _authService.CheckIfPincodeIsSetAsync();

			if (!pin)
			{
				await Navigation.GoToAsync(Routes.CREATE_PINCODE);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "InitializeAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}

	[RelayCommand]
    private void EnterPincode(string number)
	{
		try
		{
			if (Pincode.Length < 4)
			{
				Pincode+=number;
			}
		}
        catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("EnterPincode Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
    private async Task LoginAsync()
	{
		try
		{
			if (Pincode.Length == 4)
			{
				// log in
				var result = await _authService.LoginAsync(Pincode);

				if (result)
				{
					await Navigation.GoToUpwardsAsync(nameof(DashboardPage));
				}
				else
				{
					ToastHelper.ShowErrorMessage(_logger, "Invalid Pincode");
				}
			}
		}
        catch (Exception ex)
		{
			await ToastHelper.ShowDefaultErrorMessageAsync();
			_logger.LogError("BiometricLoginAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
    private void RemoveNumber()
    {
		if (Pincode.Length != 0)
		{
			Pincode = Pincode.Remove(Pincode.Length - 1);
		}
    }

	[RelayCommand]
    private async Task BiometricLoginAsync()
    {
		try
		{
			var biometric = await _authService.CheckBiometricsAsync();

			if (biometric == CheckBiometricsResult.Authenticated)
			{
				await Navigation.GoToUpwardsAsync(nameof(DashboardPage));
			}
		}
        catch (Exception ex)
		{
			await ToastHelper.ShowDefaultErrorMessageAsync();
			_logger.LogError("BiometricLoginAsync Error: {Message}", ex.Message);
		}
    }
}

using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;

namespace UC.Umc.ViewModels.Popups;

public partial class CreatePinViewModel : BaseViewModel
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ShowLogin))]
	[NotifyPropertyChangedFor(nameof(Logo))]
	private string _pincode = string.Empty;

	public string Logo => $"logo{Pincode.Length}_dark.png";
	public bool ShowLogin => Pincode.Length > 0;

	public CreatePinViewModel(ILogger<CreatePinViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private void EnterPincode(string number)
	{
		try
		{
			if (Pincode.Length < 4)
			{
				Pincode += number;
			}
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("EnterPincode Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task SubmitAsync()
	{
		try
		{
			await UserSecureStore.SetUserDataAsync(TextConstants.PINCODE_KEY, Pincode, _logger);

			ClosePopup();
		}
		catch (Exception ex)
		{
			await ToastHelper.ShowDefaultErrorMessageAsync();
			_logger.LogError("SubmitAsync Error: {Message}", ex.Message);
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
}

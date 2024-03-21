namespace UC.Umc.ViewModels.Popups;

public partial class TransferOptionsViewModel : BaseViewModel
{
	public TransferOptionsViewModel(ILogger<AuthorOptionsViewModel> logger) : base(logger)
	{
	}
	
	[RelayCommand]
	private async Task OpenUnfinishedTransferPageAsync()
	{
		try
		{
			await Navigation.GoToAsync(nameof(UnfinishTransferPage));
			ClosePopup();
		}
		catch (Exception ex)
		{
			_logger.LogError("TransferProductAsync Error: {Message}", ex.Message);
		}
	}
}

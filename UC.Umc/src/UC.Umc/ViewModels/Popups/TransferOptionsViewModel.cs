using UC.Umc.Common.Helpers;
using UC.Umc.Pages.Transactions;

namespace UC.Umc.ViewModels.Popups;

public partial class TransferOptionsViewModel : BaseViewModel
{
	public TransferOptionsViewModel(ILogger<DomainOptionsViewModel> logger) : base(logger)
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

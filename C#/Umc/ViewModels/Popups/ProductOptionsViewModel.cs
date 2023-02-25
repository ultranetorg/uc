namespace UC.Umc.ViewModels.Popups;

public partial class ProductOptionsViewModel : BaseViewModel
{
	[ObservableProperty]
    private ProductViewModel _product;

	public ProductOptionsViewModel(ILogger<AuthorOptionsViewModel> logger) : base(logger)
	{
	}
	
	[RelayCommand]
	private async Task TransferProductAsync()
	{
		try
		{
			await Navigation.GoToAsync(nameof(ProductTransferPage),
				new Dictionary<string, object>() { { QueryKeys.PRODUCT, Product } });
			ClosePopup();
		}
		catch (Exception ex)
		{
			_logger.LogError("TransferProductAsync Error: {Message}", ex.Message);
		}
	}
}

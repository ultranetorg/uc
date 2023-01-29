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
		await Navigation.GoToAsync(nameof(ProductTransferPage),
			new Dictionary<string, object>() { { QueryKeys.PRODUCT, Product } });
		await Task.Delay(10);
		ClosePopup();
	}
}

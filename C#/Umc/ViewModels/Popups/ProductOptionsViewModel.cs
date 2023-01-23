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
		//await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_TRANSFER,
		//	new Dictionary<string, object>()
		//{
		//	{ QueryKeys.AUTHOR, Product }
		//});
		ClosePopup();
	}
}

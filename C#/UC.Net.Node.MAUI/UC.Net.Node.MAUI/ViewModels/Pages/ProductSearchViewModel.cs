namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductSearchViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Product> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductSearchViewModel(ILogger<ProductSearchViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void CreateAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }

	[RelayCommand]
    private async void RestoreAsync()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }

	[RelayCommand]
    private async void ItemTappedAsync(Transaction Transaction)
    {
        if (Transaction == null) 
            return;
        if (Transaction.Status == TransactionsStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }

	[RelayCommand]
    private async void OptionsAsync(Transaction Transaction)
    {
        if (Transaction.Status == TransactionsStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }

	private void FillFakeData()
	{
        ProductsFilter = new CustomCollection<string> { "Recent", "By author" };
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
	}
}

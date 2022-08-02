namespace UC.Net.Node.MAUI.Pages;

public partial class ProductSearchPage : CustomPage
{
    public ProductSearchPage()
    {
        InitializeComponent();
        BindingContext = new ProductSearchViewModel(ServiceHelper.GetService<ILogger<ProductSearchViewModel>>());
    }
}
public class ProductSearchViewModel : BaseViewModel
{
    public ProductSearchViewModel(ILogger<ProductSearchViewModel> logger) : base(logger)
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
    public Command CreateCommand
    {
        get => new Command(Create);
    }

    private async void Create()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }
    public Command RestoreCommand
    {
        get => new Command(Restore);
    }

    private async void Restore()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }
    public Command ItemTappedCommand
    {
        get => new Command<Transaction>(ItemTapped);
    }

    private async void ItemTapped(Transaction Transaction)
    {
        if (Transaction == null) 
            return;
        if (Transaction.Status == TransactionsStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }
    public Command OptionsCommand
    {
        get => new Command<Transaction>(Options);
    }

    private async void Options(Transaction Transaction)
    {
        if (Transaction.Status == TransactionsStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }

    CustomCollection<Product> _Products = new CustomCollection<Product>();
    public CustomCollection<Product> Products
    {
        get { return _Products; }
        set { SetProperty(ref _Products, value); }
    }
    CustomCollection<string> _ProductsFilter = new CustomCollection<string>();
    public CustomCollection<string> ProductsFilter
    {
        get { return _ProductsFilter; }
        set { SetProperty(ref _ProductsFilter, value); }
    }
}

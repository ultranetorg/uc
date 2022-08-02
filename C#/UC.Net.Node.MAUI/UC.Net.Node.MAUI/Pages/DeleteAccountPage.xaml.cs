namespace UC.Net.Node.MAUI.Pages;

public partial class DeleteAccountPage : CustomPage
{
    public DeleteAccountPage(Wallet wallet)
    {
        InitializeComponent();
        BindingContext = new DeleteAccountViewModel(wallet, ServiceHelper.GetService<ILogger<DeleteAccountViewModel>>());
    }
}
public class DeleteAccountViewModel : BaseViewModel
{   
    public DeleteAccountViewModel(Wallet wallet, ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
        Wallet = wallet;          
        Authors.Add(new Author { Name = "ultranet" });
        Authors.Add(new Author { Name = "ultranetorganization" });
        Authors.Add(new Author { Name = "aximion" });
        Products.Add(new Product { Name = "UNS" });
        Products.Add(new Product { Name = "Aximion3D" });
        Products.Add(new Product { Name = "ultranet" });
    }

    public Command DeleteCommand
    {
        get => new Command(Delete);
    }

    private async void Delete()
    {
        await DeleteAccountPopup.Show(Wallet);
    }
       
    CustomCollection<Author> _Authors = new CustomCollection<Author>();
    public CustomCollection<Author> Authors
    {
        get { return _Authors; }
        set { SetProperty(ref _Authors, value); }
    }
    CustomCollection<Product> _Products = new CustomCollection<Product>();
    public CustomCollection<Product> Products
    {
        get { return _Products; }
        set { SetProperty(ref _Products, value); }
    }

    Wallet _Wallet;
    public Wallet Wallet
    {
        get { return _Wallet; }
        set { SetProperty(ref _Wallet, value); }
    }
}

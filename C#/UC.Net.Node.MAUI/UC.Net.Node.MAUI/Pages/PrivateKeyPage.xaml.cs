namespace UC.Net.Node.MAUI.Pages;

public partial class PrivateKeyPage : CustomPage
{
    public PrivateKeyPage(Wallet wallet)
    {
        InitializeComponent();
        BindingContext = new PrivateKeyViewModel(wallet, ServiceHelper.GetService<ILogger<PrivateKeyViewModel>>());
    }
}
public class PrivateKeyViewModel : BaseViewModel
{
    public PrivateKeyViewModel(Wallet wallet, ILogger<PrivateKeyViewModel> logger) : base(logger)
    {
        Wallet = wallet;   
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

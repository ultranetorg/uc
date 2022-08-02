namespace UC.Net.Node.MAUI.Pages;

public partial class ManageAccountsPage : CustomPage
{
    public ManageAccountsPage()
    {
        InitializeComponent();
        BindingContext = new ManageAccountsViewModel(ServiceHelper.GetService<ILogger<ManageAccountsViewModel>>());
    }
}
public class ManageAccountsViewModel : BaseViewModel
{
    public ManageAccountsViewModel(ILogger<ManageAccountsViewModel> logger) : base(logger)
    {
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47F0",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#6601e3"),
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "2T52",
            Name = "Primary ultranet wallet",
            AccountColor = Color.FromArgb("#3765f4"),

        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "9MDL",
            Name = "Secondary wallet",
            AccountColor = Color.FromArgb("#4cb16c"),

        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "UYO3",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#e65c93"),

        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47FO",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#ba918c"),

        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "2T52",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#d56a48"),

        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47FO",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#56d7de"),

        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "2T52",
            Name = "Main ultranet wallet",
            AccountColor = Color.FromArgb("#bb50dd"),

        });
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
        get => new Command<Wallet>(ItemTapped);
    }

    private async void ItemTapped(Wallet wallet)
    {
        if (wallet == null) 
            return;
        await Shell.Current.Navigation.PushAsync(new AccountDetailsPage(wallet));
    }
    public Command OptionsCommand
    {
        get => new Command<Wallet>(Options);
    }

    private async void Options(Wallet wallet)
    {
        await AccountOptionsPopup.Show(wallet);
    }

    Wallet _SelectedItem ;
    public Wallet SelectedItem
    {
        get { return _SelectedItem; }
        set { SetProperty(ref _SelectedItem, value); }
    }
    CustomCollection<Wallet> _Wallets =new CustomCollection<Wallet>();
    public CustomCollection<Wallet>  Wallets
    {
        get { return _Wallets; }
        set { SetProperty(ref _Wallets, value); }
    }    
}

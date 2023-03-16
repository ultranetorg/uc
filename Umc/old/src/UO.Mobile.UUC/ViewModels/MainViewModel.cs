using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Models.Transactions;
using UO.Mobile.UUC.Services.Accounts;
using UO.Mobile.UUC.Services.Authors;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.Services.Notifications;
using UO.Mobile.UUC.Services.Products;
using UO.Mobile.UUC.Services.Transactions;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels;

public class MainViewModel : BaseViewModel, IInitializableAsync
{
    public ICommand NavigateCommand => new Command<Type>(NavigateAsync);

    private int _notificationsCount;
    private ObservableCollection<Account> _accounts;
    private ObservableCollection<Transaction> _transactions;
    private int _accountsCount;
    private int _auctionsCount;
    private int _outbindedAuctionsCount;
    private int _authorsCount;
    private int _renewalNeededAuthorsCount;
    private int _productsCount;
    private int _totalReleasesCount;
    private int _cleanReleasesCount;
    private int _compomisedReleasesCount;
    private string _lastReleaseName;

    private readonly IAccountsService _accountsService;
    private readonly IAuthorsService _authorsService;
    private readonly INavigationService _navigationService;
    private readonly INotificationsService _notificationsService;
    private readonly ITransactionsService _transactionsService;
    private readonly IProductsService _productsService;

    public MainViewModel(IAccountsService accountsService, IAuthorsService authorsService,
        INavigationService navigationService, INotificationsService notificationsService,
        ITransactionsService transactionsService, IProductsService productsService)
    {
        _accountsService = accountsService;
        _authorsService = authorsService;
        _navigationService = navigationService;
        _notificationsService = notificationsService;
        _transactionsService = transactionsService;
        _productsService = productsService;
    }

    private async void Fetch()
    {
        IsBuisy = true;
        NotificationsCount = await _notificationsService.GetNotificationsCountAsync();
        Accounts = await _accountsService.GetLastAsync(3);
        Transactions = await _transactionsService.GetLastAsync(3);
        AccountsCount = await _accountsService.GetCountAsync();
        // AuctionsCount
        // OutbindedAuctionsCount
        AuthorsCount = await _authorsService.GetCountAsync();
        // RenewalNeededAuthorsCount
        ProductsCount = await _productsService.GetCountAsync();
        // TotalReleasesCount
        // CleanReleasesCount
        // CompomisedReleasesCount
        // LastReleaseName
        IsBuisy = false;
    }

    private async void NavigateAsync(Type pageType)
    {
        Guard.Against.Null(pageType, nameof(pageType));

        await _navigationService.NavigateToAsync(pageType);
    }

    public bool IsInitialized { get; set; } = false;
    public bool MultipleInitialization => true;

    public async Task InitializeAsync(object parameter)
    {
        // TODO: called twice during initialization.
        IsInitialized = false;
        Fetch();
        IsInitialized = true;
    }

    public int NotificationsCount
    {
        get => _notificationsCount;
        set
        {
            _notificationsCount = value;
            OnPropertyChanged(nameof(NotificationsCount));
        }
    }

    public ObservableCollection<Account> Accounts
    {
        get => _accounts;
        set
        {
            _accounts = value;
            OnPropertyChanged(nameof(Accounts));
        }
    }

    public ObservableCollection<Transaction> Transactions
    {
        get => _transactions;
        set
        {
            _transactions = value;
            OnPropertyChanged(nameof(Transactions));
        }
    }

    public int AccountsCount
    {
        get => _accountsCount;
        set
        {
            _accountsCount = value;
            OnPropertyChanged(nameof(AccountsCount));
        }
    }

    public int AuctionsCount
    {
        get => _auctionsCount;
        set
        {
            _auctionsCount = value;
            OnPropertyChanged(nameof(AuctionsCount));
        }
    }

    public int OutbindedAuctionsCount
    {
        get => _outbindedAuctionsCount;
        set
        {
            _outbindedAuctionsCount = value;
            OnPropertyChanged(nameof(OutbindedAuctionsCount));
        }
    }

    public int AuthorsCount
    {
        get => _authorsCount;
        set
        {
            _authorsCount = value;
            OnPropertyChanged(nameof(AuthorsCount));
        }
    }

    public int RenewalNeededAuthorsCount
    {
        get => _renewalNeededAuthorsCount;
        set
        {
            _renewalNeededAuthorsCount = value;
            OnPropertyChanged(nameof(RenewalNeededAuthorsCount));
        }
    }

    public int ProductsCount
    {
        get => _productsCount;
        set
        {
            _productsCount = value;
            OnPropertyChanged(nameof(ProductsCount));
        }
    }

    public int TotalReleasesCount
    {
        get => _totalReleasesCount;
        set
        {
            _totalReleasesCount = value;
            OnPropertyChanged(nameof(TotalReleasesCount));
        }
    }

    public int CleanReleasesCount
    {
        get => _cleanReleasesCount;
        set
        {
            _cleanReleasesCount = value;
            OnPropertyChanged(nameof(CleanReleasesCount));
        }
    }

    public int CompomisedReleasesCount
    {
        get => _compomisedReleasesCount;
        set
        {
            _compomisedReleasesCount = value;
            OnPropertyChanged(nameof(CompomisedReleasesCount));
        }
    }

    public string LastReleaseName
    {
        get => _lastReleaseName;
        set
        {
            _lastReleaseName = value;
            OnPropertyChanged(nameof(LastReleaseName));
        }
    }
}

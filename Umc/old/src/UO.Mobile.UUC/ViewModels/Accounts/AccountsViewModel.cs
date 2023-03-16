using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Pages;
using UO.Mobile.UUC.Pages.Accounts;
using UO.Mobile.UUC.Pages.Accounts.Create;
using UO.Mobile.UUC.Pages.Accounts.Restore;
using UO.Mobile.UUC.Services.Accounts;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels.Accounts;

public class AccountsViewModel : BaseViewModel, IInitializableAsync
{
    public ICommand DetailsCommand => new Command<Account>(DetailsAsync);
    public ICommand ReceiveCommand => new Command<Account>(ReceiveAsync);
    public ICommand SendCommand => new Command<Account>(SendAsync);

    public ICommand BackupCommand => new Command<Account>(BackupAsync);
    public ICommand ShowPrivateKeyCommand => new Command<Account>(ShowPrivateKeyAsync);
    public ICommand DeleteCommand => new Command<Account>(DeleteAsync);
    public ICommand HideFromDashboardCommand => new Command<Account>(HideFromDashboardAsync);

    public ICommand CreateCommand => new Command(CreateAsync);
    public ICommand RestoreCommand => new Command(RestoreAsync);

    private ObservableCollection<Account> _accounts;

    private readonly IAccountsService _accountsService;
    private readonly INavigationService _navigationService;

    public AccountsViewModel(IAccountsService accountsService, INavigationService navigationService)
    {
        _accountsService = accountsService;
        _navigationService = navigationService;
    }

    public bool IsInitialized { get; set; }
    public bool MultipleInitialization => true;

    public async Task InitializeAsync(object parameter)
    {
        IsInitialized = false;
        Accounts = await _accountsService.GetAllAsync();
        IsInitialized = true;
    }

    private async void DetailsAsync(Account account)
    {
        Guard.Against.Null(account, nameof(account));

        await _navigationService.NavigateToAsync<DetailsPage>(account);
    }

    private async void ReceiveAsync(Account account)
    {
        Guard.Against.Null(account, nameof(account));

        await _navigationService.NavigateToAsync<ReceiveFromPage>(account);
    }

    private async void SendAsync(Account account)
    {
        Guard.Against.Null(account, nameof(account));

        await _navigationService.NavigateToAsync<SendToPage>(account);
    }

    private async void BackupAsync(Account account)
    {
        Guard.Against.Null(account, nameof(account));

        await _navigationService.NavigateToAsync<BackupPage>(account);
    }

    private async void ShowPrivateKeyAsync(Account account)
    {
        Guard.Against.Null(account, nameof(account));

        await _navigationService.NavigateToAsync<ShowPrivateKeyPage>(account);
    }

    private async void DeleteAsync(Account account)
    {
        Guard.Against.Null(account, nameof(account));

        await _navigationService.NavigateToAsync<DeletePage>(account);
    }

    private async void HideFromDashboardAsync(Account account)
    {
    }

    private async void CreateAsync()
    {
        await _navigationService.NavigateToAsync<Create1Page>();
    }

    private async void RestoreAsync()
    {
        await _navigationService.NavigateToAsync<Restore1Page>();
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
}

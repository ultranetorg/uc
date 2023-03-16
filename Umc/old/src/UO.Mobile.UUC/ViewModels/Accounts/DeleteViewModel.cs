using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Services.Accounts;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels.Accounts;

internal class DeleteViewModel : BaseViewModel, IInitializableAsync
{
    public ICommand DeleteCommand => new Command(DeleteAsync);

    private Account _account;

    private readonly IAccountsService _accountsService;
    private readonly INavigationService _navigationService;

    public DeleteViewModel(IAccountsService accountsService, INavigationService navigationService)
    {
        _accountsService = accountsService;
        _navigationService = navigationService;
    }

    public bool IsInitialized { get; set; }

    public Task InitializeAsync(object parameter)
    {
        Guard.Against.Null(parameter, nameof(parameter));

        IsInitialized = false;
        Account = parameter as Account;
        IsInitialized = true;

        return Task.CompletedTask;
    }

    private async void DeleteAsync()
    {
        await _accountsService.DeleteByAddressAsync(_account.Address);
        await _navigationService.NavigateBackAsync();
    }

    public Account Account
    {
        get => _account;
        set
        {
            _account = value;
            OnPropertyChanged(nameof(Account));
        }
    }
}

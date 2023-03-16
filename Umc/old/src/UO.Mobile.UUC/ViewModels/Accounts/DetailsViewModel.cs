using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Services.Accounts;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels.Accounts;

public class DetailsViewModel : BaseViewModel, IInitializableAsync, IBackButtonPressedHandler
{
    private Account _account;

    private readonly IAccountsService _accountsService;

    public DetailsViewModel(IAccountsService accountsService)
    {
        _accountsService = accountsService;
    }

    public bool IsInitialized { get; set; }

    public Task InitializeAsync(object parameter)
    {
        IsInitialized = false;

        if (parameter is Account account)
        {
            Account = account;
        }

        IsInitialized = true;

        return Task.CompletedTask;
    }

    public void OnBackButtonPressed()
    {
        SaveAsync();
    }

    private async void SaveAsync()
    {
        await _accountsService.UpdateAsync(_account);
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

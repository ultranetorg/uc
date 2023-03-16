using UO.Mobile.UUC.Pages.Accounts;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.RestoreAccount;

namespace UO.Mobile.UUC.ViewModels.Accounts.Restore;

internal class Restore2ViewModel : BaseViewModel
{
    public ICommand CancelCommand => new Command(CancelAsync);

    public ICommand ConfirmCommand => new Command(ConfirmAsync);

    private readonly IRestoreAccountWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Restore2ViewModel(IRestoreAccountWorkflow workflow, INavigationService navigationService)
    {
        _workflow = workflow;
        _navigationService = navigationService;
    }

    private async void ConfirmAsync()
    {
        await _navigationService.NavigateToAsync<AccountsPage>();
    }

    private async void CancelAsync()
    {
        await _navigationService.NavigateToAsync<AccountsPage>();
    }
}

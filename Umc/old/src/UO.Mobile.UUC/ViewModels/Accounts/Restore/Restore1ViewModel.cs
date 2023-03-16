using UO.Mobile.UUC.Pages.Accounts.Restore;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.RestoreAccount;

namespace UO.Mobile.UUC.ViewModels.Accounts.Restore;

internal class Restore1ViewModel : BaseViewModel
{
    public ICommand NextCommand => new Command(NextAsync);

    private readonly IRestoreAccountWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Restore1ViewModel(IRestoreAccountWorkflow workflow, INavigationService navigationService)
    {
        _workflow = workflow;
        _navigationService = navigationService;

        _workflow.Initialize();
    }

    private async void NextAsync()
    {
        await _navigationService.NavigateToAsync<Restore2Page>();
    }
}

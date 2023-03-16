using UO.Mobile.UUC.Pages.Accounts;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.CreateAccount;

namespace UO.Mobile.UUC.ViewModels.Accounts.Create;

internal class Create3ViewModel : BaseViewModel
{
    public ICommand CancelCommand => new Command(CancelAsync);

    public ICommand ConfirmCommand => new Command(ConfirmAsync);

    private readonly ICreateAccountWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Create3ViewModel(ICreateAccountWorkflow workflow, INavigationService navigationService)
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

    public string Name
    {
        get => _workflow.Name;
        set
        {
            _workflow.Name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
}

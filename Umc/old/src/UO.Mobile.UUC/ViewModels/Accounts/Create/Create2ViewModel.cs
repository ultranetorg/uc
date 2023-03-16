using UO.Mobile.UUC.Pages.Accounts.Create;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.CreateAccount;

namespace UO.Mobile.UUC.ViewModels.Accounts.Create;

public class Create2ViewModel : BaseViewModel
{
    public ICommand NextCommand => new Command(NextAsync);

    private readonly ICreateAccountWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Create2ViewModel(ICreateAccountWorkflow workflow, INavigationService navigationService)
    {
        _workflow = workflow;
        _navigationService = navigationService;
    }

    private async void NextAsync()
    {
        await _navigationService.NavigateToAsync<Create3Page>();
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

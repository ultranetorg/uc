using UO.Mobile.UUC.Pages.Accounts.Create;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.CreateAccount;

namespace UO.Mobile.UUC.ViewModels.Accounts.Create;

public class Create1ViewModel : BaseViewModel
{
    public ICommand NextCommand => new Command(NextAsync);

    private readonly ICreateAccountWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Create1ViewModel(ICreateAccountWorkflow workflow, INavigationService navigationService)
    {
        _workflow = workflow;
        _navigationService = navigationService;

        _workflow.Initialize();
    }

    private async void NextAsync()
    {
        await _navigationService.NavigateToAsync<Create2Page>();
    }

    public string Password
    {
        get => _workflow.Password;
        set
        {
            _workflow.Name = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    public string PasswordConfirm
    {
        get => _workflow.PasswordConfirm;
        set
        {
            _workflow.PasswordConfirm = value;
            OnPropertyChanged(nameof(_workflow.PasswordConfirm));
        }
    }
}

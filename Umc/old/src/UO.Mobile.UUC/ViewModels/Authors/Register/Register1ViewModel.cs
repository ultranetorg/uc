using UO.Mobile.UUC.Pages.Authors.Register;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.RegisterAuthor;

namespace UO.Mobile.UUC.ViewModels.Authors.Register;

internal class Register1ViewModel : BaseViewModel
{
    public ICommand NextCommand => new Command(NextAsync);

    private readonly IRegisterAuthorWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Register1ViewModel(IRegisterAuthorWorkflow workflow, INavigationService navigationService)
    {
        _workflow = workflow;
        _navigationService = navigationService;

        _workflow.Initialize();
    }

    private async void NextAsync()
    {
        await _navigationService.NavigateToAsync<Register2Page>();
    }
}

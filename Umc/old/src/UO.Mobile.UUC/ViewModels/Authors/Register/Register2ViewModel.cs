using UO.Mobile.UUC.Pages.Authors;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;
using UO.Mobile.UUC.Workflows.RegisterAuthor;

namespace UO.Mobile.UUC.ViewModels.Authors.Register;

internal class Register2ViewModel : BaseViewModel
{
    public ICommand CancelCommand => new Command(CancelAsync);

    public ICommand ConfirmCommand => new Command(ConfirmAsync);

    private readonly IRegisterAuthorWorkflow _workflow;
    private readonly INavigationService _navigationService;

    public Register2ViewModel(IRegisterAuthorWorkflow workflow, INavigationService navigationService)
    {
        _workflow = workflow;
        _navigationService = navigationService;
    }

    private async void ConfirmAsync()
    {
        await _navigationService.NavigateToAsync<AuthorsPage>();
    }

    private async void CancelAsync()
    {
        await _navigationService.NavigateToAsync<AuthorsPage>();
    }
}

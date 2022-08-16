using CreateAccountViewModel = UC.Net.Node.MAUI.ViewModels.Pages.CreateAccountViewModel;
namespace UC.Net.Node.MAUI.Pages;

public partial class CreateAccountPage : CustomPage
{
    public CreateAccountPage(CreateAccountViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<CreateAccountViewModel>();
    }
}

using CreateAccountViewModel = UC.Net.Node.MAUI.ViewModels.Pages.CreateAccountViewModel;

namespace UC.Net.Node.MAUI.Pages;

public partial class CreateAccountPage : CustomPage
{
    public CreateAccountPage()
    {
        InitializeComponent();
        BindingContext = new CreateAccountViewModel(ServiceHelper.GetService<ILogger<CreateAccountViewModel>>());
    }
}

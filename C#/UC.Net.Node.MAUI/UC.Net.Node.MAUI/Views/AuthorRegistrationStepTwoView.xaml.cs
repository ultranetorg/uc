namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistrationStepTwoView : ContentView
{
    public AuthorRegistrationStepTwoView()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationStepTwoViewModel(ServiceHelper.GetService<ILogger<AuthorRegistrationStepTwoViewModel>>());
    }
}

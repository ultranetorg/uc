namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistrationRenewalStepTwoView : ContentView
{
    public AuthorRegistrationRenewalStepTwoView()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationRenewalStepTwoViewModel(ServiceHelper.GetService<ILogger<AuthorRegistrationRenewalStepTwoViewModel>>());
    }
}

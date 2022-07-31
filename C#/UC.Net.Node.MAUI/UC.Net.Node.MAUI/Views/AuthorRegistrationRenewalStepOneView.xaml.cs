namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistrationRenewalStepOneView : ContentView
{
    public AuthorRegistrationRenewalStepOneView()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationRenewalStepOneViewModel(ServiceHelper.GetService<ILogger<AuthorRegistrationRenewalStepOneViewModel>>());
    }
}
namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistrationStepOneView : ContentView
{
    public AuthorRegistrationStepOneView()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationStepOneViewModel(ServiceHelper.GetService<ILogger<AuthorRegistrationStepOneViewModel>>());
    }
}

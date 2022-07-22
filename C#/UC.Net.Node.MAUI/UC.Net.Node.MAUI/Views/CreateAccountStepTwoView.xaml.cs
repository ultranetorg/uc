namespace UC.Net.Node.MAUI.Views;
public partial class CreateAccountStepTwoView : ContentView
{
    public CreateAccountStepTwoView()
    {
        InitializeComponent();
        BindingContext = new CreateAccountViewModel(ServiceHelper.GetService<ILogger<CreateAccountViewModel>>());
    }
}
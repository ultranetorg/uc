namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinPage : CustomPage
{
    public EnterPinPage()
    {
        InitializeComponent();
        BindingContext = new EnterPinViewModel(ServiceHelper.GetService<ILogger<EnterPinViewModel>>());
    }
}

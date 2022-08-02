namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinBPage : CustomPage
{
    public EnterPinBPage()
    {
        InitializeComponent();
        BindingContext = new EnterPinBViewModel(ServiceHelper.GetService<ILogger<EnterPinBViewModel>>());
    }
}

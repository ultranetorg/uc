namespace UC.Net.Node.MAUI.Pages;

public partial class NetworkPage : CustomPage
{
    public NetworkPage()
    {
        InitializeComponent();
        BindingContext = new NetworkViewModel(ServiceHelper.GetService<ILogger<NetworkViewModel>>());
    }
}

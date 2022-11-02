namespace UC.Umc.Pages;

public partial class NetworkPage : CustomPage
{
    public NetworkPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<NetworkViewModel>();
    }

    public NetworkPage(NetworkViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

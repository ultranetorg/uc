namespace UC.Net.Node.MAUI.Pages;

public partial class AboutPage : CustomPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = new AboutViewModel(ServiceHelper.GetService<ILogger<AboutViewModel>>());
    }
}

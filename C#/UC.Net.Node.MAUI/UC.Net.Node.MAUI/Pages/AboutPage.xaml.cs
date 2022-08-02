namespace UC.Net.Node.MAUI.Pages;

public partial class AboutPage : CustomPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = new AboutViewModel(this, ServiceHelper.GetService<ILogger<AboutViewModel>>());
    }
}

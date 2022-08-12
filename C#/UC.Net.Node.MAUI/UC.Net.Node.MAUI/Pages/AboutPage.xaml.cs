namespace UC.Net.Node.MAUI.Pages;

public partial class AboutPage : CustomPage
{
    public AboutPage()
    {
        InitializeComponent();
		var service = App.ServiceProvider.GetService<ILogger<AboutViewModel>>();
        BindingContext = new AboutViewModel(service);
    }
}

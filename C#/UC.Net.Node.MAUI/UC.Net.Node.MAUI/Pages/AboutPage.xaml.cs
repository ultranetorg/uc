namespace UC.Net.Node.MAUI.Pages;

public partial class AboutPage : CustomPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AboutViewModel>();
    }

    public AboutPage(AboutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

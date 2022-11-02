namespace UC.Net.Node.MAUI.Pages;

public partial class AboutPage : CustomPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AboutViewModel>();
    }

    public AboutPage(AboutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

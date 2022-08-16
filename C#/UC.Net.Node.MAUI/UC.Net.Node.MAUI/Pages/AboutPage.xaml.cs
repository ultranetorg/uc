namespace UC.Net.Node.MAUI.Pages;

public partial class AboutPage : CustomPage
{
    public AboutPage(AboutViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

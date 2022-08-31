namespace UC.Net.Node.MAUI.Pages;

public partial class HelpBPage : CustomPage
{
    public HelpBPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<HelpBViewModel>();
    }

    public HelpBPage(HelpBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

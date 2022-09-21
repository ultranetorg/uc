namespace UC.Net.Node.MAUI.Pages;

public partial class HelpBPage : CustomPage
{
    public HelpBPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<HelpBViewModel>();
    }

    public HelpBPage(HelpBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

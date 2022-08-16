namespace UC.Net.Node.MAUI.Pages;

public partial class HelpBPage : CustomPage
{
    public HelpBPage(HelpBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

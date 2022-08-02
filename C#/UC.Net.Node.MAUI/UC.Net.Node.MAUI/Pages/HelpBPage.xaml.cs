namespace UC.Net.Node.MAUI.Pages;

public partial class HelpBPage : CustomPage
{
    public HelpBPage()
    {
        InitializeComponent();
        BindingContext = new HelpBViewModel(ServiceHelper.GetService<ILogger<HelpBViewModel>>());
    }
}

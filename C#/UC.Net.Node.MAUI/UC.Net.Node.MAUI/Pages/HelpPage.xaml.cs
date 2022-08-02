namespace UC.Net.Node.MAUI.Pages;

public partial class HelpPage : CustomPage
{
    public HelpPage()
    {
        InitializeComponent();
        BindingContext = new HelpViewModel(ServiceHelper.GetService<ILogger<HelpViewModel>>());
    }
}

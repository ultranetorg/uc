namespace UC.Net.Node.MAUI.Pages;

public partial class WhatsNewPage : CustomPage
{
    public WhatsNewPage()
    {
        InitializeComponent();
        BindingContext = new WhatsNewViewModel(ServiceHelper.GetService<ILogger<WhatsNewViewModel>>());
    }
}

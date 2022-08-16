namespace UC.Net.Node.MAUI.Pages;

public partial class WhatsNewPage : CustomPage
{
    public WhatsNewPage(WhatsNewViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

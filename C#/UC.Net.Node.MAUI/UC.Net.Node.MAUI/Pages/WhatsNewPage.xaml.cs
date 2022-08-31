namespace UC.Net.Node.MAUI.Pages;

public partial class WhatsNewPage : CustomPage
{
    public WhatsNewPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<WhatsNewViewModel>();
    }

    public WhatsNewPage(WhatsNewViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

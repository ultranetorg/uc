namespace UC.Net.Node.MAUI.Pages;

public partial class HelpPage : CustomPage
{
    public HelpPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<HelpViewModel>();
    }

    public HelpPage(HelpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

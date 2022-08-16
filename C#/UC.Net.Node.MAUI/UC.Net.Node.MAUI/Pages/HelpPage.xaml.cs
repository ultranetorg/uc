namespace UC.Net.Node.MAUI.Pages;

public partial class HelpPage : CustomPage
{
    public HelpPage(HelpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

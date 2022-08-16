namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinBPage : CustomPage
{
    public EnterPinBPage(EnterPinBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinPage : CustomPage
{
    public EnterPinPage(EnterPinViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

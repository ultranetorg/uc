namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinBPage : CustomPage
{
    public EnterPinBPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<EnterPinBViewModel>();
    }

    public EnterPinBPage(EnterPinBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

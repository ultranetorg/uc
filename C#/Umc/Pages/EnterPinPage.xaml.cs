namespace UC.Net.Node.MAUI.Pages;

public partial class EnterPinPage : CustomPage
{
    public EnterPinPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<EnterPinViewModel>();
    }

    public EnterPinPage(EnterPinViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

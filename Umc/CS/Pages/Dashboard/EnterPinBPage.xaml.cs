namespace UC.Umc.Pages;

public partial class EnterPinBPage : CustomPage
{
    public EnterPinBPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<EnterPinBViewModel>();
    }

    public EnterPinBPage(EnterPinBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

namespace UC.Umc.Pages;

public partial class PrivateKeyPage : CustomPage
{
    public PrivateKeyPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<PrivateKeyViewModel>();
    }

    public PrivateKeyPage(PrivateKeyViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

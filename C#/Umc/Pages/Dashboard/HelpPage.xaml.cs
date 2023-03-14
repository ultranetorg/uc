namespace UC.Umc.Pages;

public partial class HelpPage : CustomPage
{
    public HelpPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<HelpViewModel>();
    }

    public HelpPage(HelpViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

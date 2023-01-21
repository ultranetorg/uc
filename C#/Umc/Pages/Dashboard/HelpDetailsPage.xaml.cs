namespace UC.Umc.Pages;

public partial class HelpDetailsPage : CustomPage
{
    public HelpDetailsPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<HelpDetailsViewModel>();
    }

    public HelpDetailsPage(HelpDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

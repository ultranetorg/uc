namespace UC.Umc.Pages;

public partial class MakeBidPage : CustomPage
{
    public MakeBidPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<MakeBidViewModel>();
    }

    public MakeBidPage(MakeBidViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

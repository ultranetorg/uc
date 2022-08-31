namespace UC.Net.Node.MAUI.Pages;

public partial class MakeBidPage : CustomPage
{
    public MakeBidPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<MakeBidViewModel>();
    }

    public MakeBidPage(MakeBidViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

namespace UC.Net.Node.MAUI.Pages;

public partial class MakeBidPage : CustomPage
{
    public MakeBidPage(MakeBidViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<MakeBidViewModel>();
    }
}

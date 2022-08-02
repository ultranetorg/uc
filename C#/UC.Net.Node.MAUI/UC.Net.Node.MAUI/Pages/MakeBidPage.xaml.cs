namespace UC.Net.Node.MAUI.Pages;

public partial class MakeBidPage : CustomPage
{
    public MakeBidPage()
    {
        InitializeComponent();
        BindingContext = new MakeBidViewModel(ServiceHelper.GetService<ILogger<MakeBidViewModel>>());
    }
}

namespace UC.Net.Node.MAUI.Views;

public partial class MakeBid2View : ContentView
{
    public MakeBid2View()
    {
        InitializeComponent();
        BindingContext = new MakeBid2ViewModel(ServiceHelper.GetService<ILogger<MakeBid2ViewModel>>());
    }
}

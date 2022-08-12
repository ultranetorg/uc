namespace UC.Net.Node.MAUI.Views;

public partial class MakeBid1View : ContentView
{
    public MakeBid1View()
    {
        InitializeComponent();
        BindingContext = new MakeBid1ViewModel(ServiceHelper.GetService<ILogger<MakeBid1ViewModel>>());
    }
}
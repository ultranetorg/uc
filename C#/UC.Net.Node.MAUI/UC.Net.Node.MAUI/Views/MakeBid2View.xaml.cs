namespace UC.Net.Node.MAUI.Views;

public partial class MakeBid2View : ContentView
{
    public MakeBid2View(MakeBid2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public MakeBid2View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<MakeBid2ViewModel>();
    }
}

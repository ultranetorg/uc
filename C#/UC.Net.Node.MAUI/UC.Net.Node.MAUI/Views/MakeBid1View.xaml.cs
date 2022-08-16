namespace UC.Net.Node.MAUI.Views;

public partial class MakeBid1View : ContentView
{
    public MakeBid1View(MakeBid1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public MakeBid1View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<MakeBid1ViewModel>();
    }
}
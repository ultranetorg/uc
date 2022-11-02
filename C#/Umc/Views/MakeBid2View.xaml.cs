namespace UC.Umc.Views;

public partial class MakeBid2View : ContentView
{
    public MakeBid2View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<MakeBid2ViewModel>();
    }

    public MakeBid2View(MakeBid2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

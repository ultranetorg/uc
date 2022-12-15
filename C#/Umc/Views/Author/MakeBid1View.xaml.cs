namespace UC.Umc.Views;

public partial class MakeBid1View : ContentView
{
    public MakeBid1View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<MakeBid1ViewModel>();
    }

    public MakeBid1View(MakeBid1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
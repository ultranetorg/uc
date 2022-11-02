namespace UC.Umc.Views;

public partial class Send2View : ContentView
{
    public Send2View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<Send2ViewModel>();
    }

    public Send2View(Send2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

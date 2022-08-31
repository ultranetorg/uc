namespace UC.Net.Node.MAUI.Views;

public partial class Send2View : ContentView
{
    public Send2View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<Send2ViewModel>();
    }

    public Send2View(Send2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

namespace UC.Net.Node.MAUI.Views;

public partial class Send1View : ContentView
{
    public Send1View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<Send1ViewModel>();
    }

    public Send1View(Send1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

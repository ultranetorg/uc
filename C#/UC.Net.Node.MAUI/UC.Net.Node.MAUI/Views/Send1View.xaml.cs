namespace UC.Net.Node.MAUI.Views;

public partial class Send1View : ContentView
{
    public Send1View(Send1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public Send1View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<Send1ViewModel>();
    }
}

namespace UC.Net.Node.MAUI.Views;

public partial class Send1View : ContentView
{
    public Send1View()
    {
        InitializeComponent();
        BindingContext = new Send1ViewModel(ServiceHelper.GetService<ILogger<Send1ViewModel>>());
    }
}

namespace UC.Net.Node.MAUI.Views;

public partial class Send2View : ContentView
{
    public Send2View()
    {
        InitializeComponent();
        BindingContext = new Send2ViewModel(ServiceHelper.GetService<ILogger<Send2ViewModel>>()); 
    }
}

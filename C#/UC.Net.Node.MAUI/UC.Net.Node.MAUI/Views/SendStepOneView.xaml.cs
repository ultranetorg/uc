namespace UC.Net.Node.MAUI.Views;

public partial class SendStepOneView : ContentView
{
    public SendStepOneView()
    {
        InitializeComponent();
        BindingContext = new SendStepOneViewModel(ServiceHelper.GetService<ILogger<SendStepOneViewModel>>());
    }
}

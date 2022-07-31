namespace UC.Net.Node.MAUI.Views;

public partial class SendStepTwoView : ContentView
{
    public SendStepTwoView()
    {
        InitializeComponent();
        BindingContext = new SendStepTwoViewModel(ServiceHelper.GetService<ILogger<SendStepTwoViewModel>>()); 
    }
}

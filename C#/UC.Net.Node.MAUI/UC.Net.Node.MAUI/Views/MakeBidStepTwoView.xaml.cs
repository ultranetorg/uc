namespace UC.Net.Node.MAUI.Views;

public partial class MakeBidStepTwoView : ContentView
{
    public MakeBidStepTwoView()
    {
        InitializeComponent();
        BindingContext = new MakeBidStepTwoViewModel(ServiceHelper.GetService<ILogger<MakeBidStepTwoViewModel>>());
    }
}

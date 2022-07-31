namespace UC.Net.Node.MAUI.Views;

public partial class MakeBidStepOneView : ContentView
{
    public MakeBidStepOneView()
    {
        InitializeComponent();
        BindingContext = new MakeBidStepOneViewModel(ServiceHelper.GetService<ILogger<MakeBidStepOneViewModel>>());
    }
}
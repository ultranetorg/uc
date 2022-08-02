namespace UC.Net.Node.MAUI.Pages;

public partial class SendPage : CustomPage
{
    public SendPage()
    {
        InitializeComponent();
        BindingContext = new SendViewModel(ServiceHelper.GetService<ILogger<SendViewModel>>());
    }
}

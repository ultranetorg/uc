namespace UC.Net.Node.MAUI.Pages;

public partial class SendPage : CustomPage
{
    public SendPage()
    {
        InitializeComponent();
        BindingContext = new SendViewModel(this, ServiceHelper.GetService<ILogger<SendViewModel>>());
    }
}

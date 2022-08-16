namespace UC.Net.Node.MAUI.Pages;

public partial class SendPage : CustomPage
{
    public SendPage(SendViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<SendViewModel>();
    }
}

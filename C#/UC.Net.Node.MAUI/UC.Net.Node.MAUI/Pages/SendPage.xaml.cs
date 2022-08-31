namespace UC.Net.Node.MAUI.Pages;

public partial class SendPage : CustomPage
{
    public SendPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<SendViewModel>();
    }

    public SendPage(SendViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

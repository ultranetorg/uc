namespace UC.Umc.Pages;

public partial class SendPage : CustomPage
{
    public SendPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<SendViewModel>();
    }

    public SendPage(SendViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

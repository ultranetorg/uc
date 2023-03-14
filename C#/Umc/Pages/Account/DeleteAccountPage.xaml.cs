namespace UC.Umc.Pages;

public partial class DeleteAccountPage : CustomPage
{
	public DeleteAccountPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<DeleteAccountViewModel>();
    }

	public DeleteAccountPage(DeleteAccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

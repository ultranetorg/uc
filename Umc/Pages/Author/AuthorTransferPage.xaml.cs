namespace UC.Umc.Pages;

public partial class AuthorTransferPage : CustomPage
{
	public AuthorTransferPage()
	{
		InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorTransferViewModel>();
    }

    public AuthorTransferPage(AuthorTransferViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
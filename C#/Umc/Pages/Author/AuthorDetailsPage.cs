namespace UC.Umc.Pages;

public partial class AuthorDetailsPage : CustomPage
{
    public AuthorDetailsPage(AuthorViewModel author, AuthorDetailsViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }

    public AuthorDetailsPage(AuthorViewModel author)
    {
        InitializeComponent();
		var vm = Ioc.Default.GetService<AuthorDetailsViewModel>();
		vm.Author = author;
        BindingContext = vm;
    }

	public AuthorDetailsPage()
	{
        InitializeComponent();
		var vm = Ioc.Default.GetService<AuthorDetailsViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
	}
}

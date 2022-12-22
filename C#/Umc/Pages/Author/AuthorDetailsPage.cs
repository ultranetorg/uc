namespace UC.Umc.Pages;

public partial class AuthorDetailsPage : CustomPage
{
    public AuthorDetailsPage(AuthorViewModel author, AuthorDetailsPViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }

    public AuthorDetailsPage(AuthorViewModel author)
    {
        InitializeComponent();
		var vm = Ioc.Default.GetService<AuthorDetailsPViewModel>();
		vm.Author = author;
        BindingContext = vm;
    }

	public AuthorDetailsPage()
	{
        InitializeComponent();
		var vm = Ioc.Default.GetService<AuthorDetailsPViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
	}
}

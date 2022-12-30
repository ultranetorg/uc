namespace UC.Umc.Popups;

public partial class AuthorOptionsPopup : Popup
{
	AuthorOptionsViewModel Vm => BindingContext as AuthorOptionsViewModel;

    public AuthorOptionsPopup(AuthorViewModel author)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorOptionsViewModel>();
		Vm.Author = author;
		Vm.Popup = this;
    }
}
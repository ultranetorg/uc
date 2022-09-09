namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorsPage : CustomPage
{
	AuthorsViewModel Vm => BindingContext as AuthorsViewModel;
    public AuthorsPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AuthorsViewModel>();
    }

    public AuthorsPage(AuthorsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Vm.InitializeAsync();
    }
}

namespace UC.Umc.Pages;

public partial class CreateAccountPage : CustomPage
{
	CreateAccountPageViewModel Vm => BindingContext as CreateAccountPageViewModel;

	public CreateAccountPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreateAccountPageViewModel>();
    }

    public CreateAccountPage(CreateAccountPageViewModel vm)
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

namespace UC.Umc.Pages;

public partial class RestoreAccountPage : CustomPage
{
	RestoreAccountViewModel Vm => BindingContext as RestoreAccountViewModel;

	public RestoreAccountPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<RestoreAccountViewModel>();
    }

    public RestoreAccountPage(RestoreAccountViewModel vm)
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

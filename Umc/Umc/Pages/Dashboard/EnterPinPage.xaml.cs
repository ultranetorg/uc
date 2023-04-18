namespace UC.Umc.Pages;

public partial class EnterPinPage : ContentPage
{
	EnterPinViewModel Vm => BindingContext as EnterPinViewModel;

    public EnterPinPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<EnterPinViewModel>();
    }

    public EnterPinPage(EnterPinViewModel vm)
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

using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;

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

		// uncomment to test create pincode popup
		UserSecureStore.RemoveData(TextConstants.PINCODE_KEY);

        await Vm.InitializeAsync();
    }
}

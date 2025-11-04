namespace UC.Umc.Pages;

public partial class CreatePinPage : ContentPage
{
	public CreatePinPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreatePinViewModel>();
	}
}
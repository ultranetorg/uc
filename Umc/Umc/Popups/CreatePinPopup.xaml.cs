namespace UC.Umc.Popups;

public partial class CreatePinPopup : Popup
{
    public CreatePinPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreatePinViewModel>();
    }
}
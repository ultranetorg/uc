namespace UC.Umc.Popups;

public partial class SelectAuthorPopup : Popup
{
	public SelectAuthorViewModel Vm => BindingContext as SelectAuthorViewModel;

    public SelectAuthorPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<SelectAuthorViewModel>();
		Vm.Popup = this;
    }
}
namespace UC.Umc.Popups;

public partial class SourceAccountPopup : Popup
{
	public SourceAccountViewModel Vm => BindingContext as SourceAccountViewModel;

    public SourceAccountPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<SourceAccountViewModel>();
		Vm.Popup = this;
    }

    public SourceAccountPopup(bool addAllOption = false)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<SourceAccountViewModel>();
		Vm.Popup = this;
		if (addAllOption) Vm.AddAllOption();
    }
}

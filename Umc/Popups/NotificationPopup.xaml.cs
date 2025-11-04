namespace UC.Umc.Popups;

public partial class NotificationPopup : Popup
{
	NotificationViewModel Vm => BindingContext as NotificationViewModel;

    public NotificationPopup(Notification item)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<NotificationViewModel>();
		Vm.Notification = item;
		Vm.Popup = this;
    }
}
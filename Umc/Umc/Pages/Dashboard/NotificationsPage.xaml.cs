namespace UC.Umc.Pages;

public partial class NotificationsPage : CustomPage
{
    public NotificationsPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<NotificationsViewModel>();
    }
}
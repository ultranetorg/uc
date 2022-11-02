namespace UC.Umc.Pages;

public partial class SettingsPage : CustomPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<SettingsViewModel>();
    }

    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

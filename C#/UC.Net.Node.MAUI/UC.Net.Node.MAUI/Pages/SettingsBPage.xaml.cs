namespace UC.Net.Node.MAUI.Pages;

public partial class SettingsBPage : CustomPage
{
    public SettingsBPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<SettingsBViewModel>();
    }

    public SettingsBPage(SettingsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

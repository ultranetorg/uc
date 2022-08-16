namespace UC.Net.Node.MAUI.Pages;

public partial class SettingsBPage : CustomPage
{
    public SettingsBPage(SettingsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}

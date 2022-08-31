namespace UC.Net.Node.MAUI.Pages;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
		BindingContext = App.ServiceProvider.GetService<StartViewModel>();
    }

    public StartPage(StartViewModel vm)
    {
        InitializeComponent();
		BindingContext = vm;
    }
}
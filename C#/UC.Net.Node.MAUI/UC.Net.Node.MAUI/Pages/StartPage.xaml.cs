namespace UC.Net.Node.MAUI.Pages;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
		BindingContext = Ioc.Default.GetService<StartViewModel>();
    }

    public StartPage(StartViewModel vm)
    {
        InitializeComponent();
		BindingContext = vm;
    }
}
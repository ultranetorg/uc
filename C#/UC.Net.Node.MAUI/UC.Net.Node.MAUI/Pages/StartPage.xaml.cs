namespace UC.Net.Node.MAUI.Pages;

public partial class StartPage : ContentPage
{
    public StartPage(StartViewModel vm)
    {
        InitializeComponent();

		BindingContext = vm;
    }
}
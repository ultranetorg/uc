namespace UC.Net.Node.MAUI.Pages;

public partial class RestoreAccountPage : CustomPage
{
    public RestoreAccountPage()
    {
        InitializeComponent();
        BindingContext = new RestoreAccountViewModel(ServiceHelper.GetService<ILogger<RestoreAccountViewModel>>());
    }
}

public partial class RestoreAccountViewModel : BaseAccountViewModel
{
    public RestoreAccountViewModel(ILogger<RestoreAccountViewModel> logger) : base(logger)
    {
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#6601e3"), BoderColor= Shell.Current.BackgroundColor });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#d56a48"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = Colors.Transparent });
    }

	[RelayCommand]
    private async void ClosePageAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}

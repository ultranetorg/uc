namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AccountDetailsViewModel : BaseAccountViewModel
{
    public AccountDetailsViewModel(ILogger<AccountDetailsViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void SendAsync()
    {
        await Shell.Current.Navigation.PushAsync(new SendPage());
    }

	[RelayCommand]
    private async void ShowKeyAsync()
    {
        await Shell.Current.Navigation.PushAsync(new PrivateKeyPage(Wallet));
    }

	[RelayCommand]
    private async void DeleteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new DeleteAccountPage(Wallet));
    }

	private void FillFakeData()
	{
		ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#6601e3") ,BoderColor = Shell.Current.BackgroundColor });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#3765f4"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#4cb16c"),  BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#ba918c"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#d56a48"),  BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#56d7de"), BoderColor = Colors.Transparent });
        ColorsCollection.Add(new AccountColor { Color = Color.FromArgb("#bb50dd"), BoderColor = Colors.Transparent });
        Authors.Add(new Author { Name = "ultranet" });
        Authors.Add(new Author { Name = "ultranetorganization" });
        Authors.Add(new Author { Name = "aximion" });
        Products.Add(new Product { Name = "UNS" });
        Products.Add(new Product { Name = "Aximion3D" });
        Products.Add(new Product { Name = "ultranet" });
	}
}

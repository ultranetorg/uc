namespace UC.Umc.ViewModels.Popups;

public partial class AccountOptionsViewModel : ObservableObject
{
	[ObservableProperty]
    public AccountViewModel _account;

	[RelayCommand]
	private async Task Send()
	{
		await Shell.Current.Navigation.PushAsync(new SendPage());
	}
}

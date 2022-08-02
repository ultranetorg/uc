namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class HelpViewModel : BaseViewModel
{
	[ObservableProperty]
    private Transaction _selectedItem ;

	[ObservableProperty]
    private CustomCollection<string> _helps = new();

    public HelpViewModel(ILogger<HelpViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	[RelayCommand]
    private async void CreateAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }

	[RelayCommand]
    private async void RestoreAsync()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }

	[RelayCommand]
    private void ItemTappedAsync(string help)
    { 
    }

	private void FillFakeData()
	{
        Helps.Add("How to turn animation on or off in the search bar");
        Helps.Add("How to quickly change Android phone settings");
        Helps.Add("How to understand which settings the icons correspond to the following icons are located in the upper right corner of the screen or in the quick settings panel");
        Helps.Add("The quick settings panel makes it easy to navigate and customize options");
        Helps.Add("How to turn animation on or off in the search bar");
        Helps.Add("How to quickly change Android phone settings");
        Helps.Add("How to understand which settings the icons correspond to the following icons are located in the upper right corner of the screen or in the quick settings panel");
        Helps.Add("The quick settings panel makes it easy to navigate and customize options");
        Helps.Add("How to turn animation on or off in the search bar");
        Helps.Add("How to quickly change Android phone settings");
        Helps.Add("How to understand which settings the icons correspond to the following icons are located in the upper right corner of the screen or in the quick settings panel");
        Helps.Add("The quick settings panel makes it easy to navigate and customize options");
        Helps.Add("How to turn animation on or off in the search bar");
	}
}

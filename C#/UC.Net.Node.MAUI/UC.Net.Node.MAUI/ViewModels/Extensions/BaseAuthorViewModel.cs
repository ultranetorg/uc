namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseAuthorViewModel : BaseViewModel
{
    public Author Author { get; set;}

	[ObservableProperty]
    private bool _isRegistered;

	protected BaseAuthorViewModel(ILogger logger) : base(logger){}
	
	[RelayCommand]
    private async void CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	[RelayCommand]
    private async void MakeBidAsync()
    {
        await Shell.Current.Navigation.PushAsync(new MakeBidPage());
    }

	[RelayCommand]
    private void Register()
    {
        IsRegistered = true;
    }
}

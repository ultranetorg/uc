namespace UC.Umc.ViewModels;

public abstract partial class BaseAuthorViewModel : BaseViewModel
{
    public Author Author { get; set;}

	[ObservableProperty]
    private bool _isRegistered;

	protected BaseAuthorViewModel(ILogger logger) : base(logger){}
	
	[RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

	[RelayCommand]
    private async Task MakeBidAsync()
    {
        await Shell.Current.Navigation.PushAsync(new MakeBidPage());
    }

	[RelayCommand]
    private void Register()
    {
        IsRegistered = true;
    }
}

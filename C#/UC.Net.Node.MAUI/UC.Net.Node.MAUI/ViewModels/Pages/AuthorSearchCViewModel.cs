namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorSearchCViewModel : BaseViewModel
{
    public Author Author { get; }

	[ObservableProperty]
    private bool _isRegistered;

    public AuthorSearchCViewModel(Author author, ILogger<AuthorSearchCViewModel> logger) : base(logger)
    {
        Author = author;
    }

	[RelayCommand]
    private async void Cancel()
    {
        await Shell.Current.Navigation.PopAsync();
    }
	
	[RelayCommand]
    private async void MakeBid()
    {
        await Shell.Current.Navigation.PushAsync(new MakeBidPage());
    }
	
	[RelayCommand]
    private void Register()
    {
        IsRegistered = true;
    }
}

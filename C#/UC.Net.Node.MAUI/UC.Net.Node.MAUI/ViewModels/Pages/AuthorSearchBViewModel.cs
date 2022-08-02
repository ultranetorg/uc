namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorSearchBViewModel : BaseViewModel
{
    public Author Author { get; }

	[ObservableProperty]
    private bool _isRegistered;

    public AuthorSearchBViewModel(Author author, ILogger<AuthorSearchBViewModel> logger) : base(logger)
    {
        Author = author;
    }

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
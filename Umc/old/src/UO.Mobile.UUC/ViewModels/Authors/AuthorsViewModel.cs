using UO.Mobile.UUC.Models;
using UO.Mobile.UUC.Pages.Authors.Register;
using UO.Mobile.UUC.Services.Authors;
using UO.Mobile.UUC.Services.Navigation;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels.Authors;

internal class AuthorsViewModel : BaseViewModel, IInitializableAsync
{
    public ICommand RegisterCommand => new Command(RegisterAsync);

    // Bindings
    private ObservableCollection<Author> _authors;

    // Services.
    private readonly IAuthorsService _authorsService;
    private readonly INavigationService _navigationService;

    public AuthorsViewModel(IAuthorsService authorsService, INavigationService navigationService)
    {
        _authorsService = authorsService;
        _navigationService = navigationService;
    }

    // IInitializableAsync
    public bool IsInitialized { get; set; }

    public bool MultipleInitialization => true;

    public async Task InitializeAsync(object parameter)
    {
        IsInitialized = false;
        Authors = await _authorsService.GetAllAsync();
        IsInitialized = true;
    }

    public async void RegisterAsync()
    {
        await _navigationService.NavigateToAsync<Register1Page>();
    }

    // Bindings
    public ObservableCollection<Author> Authors
    {
        get => _authors;
        set
        {
            _authors = value;
            OnPropertyChanged(nameof(Authors));
        }
    }
}

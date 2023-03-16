namespace UO.Mobile.UUC.Services.Navigation;

public interface INavigationService
{
    Task NavigateToAsync([NotNull] Type pageType);

    Task NavigateToAsync<T>() where T : Page;

    Task NavigateToAsync<T>(object parameter) where T : Page;

    Task NavigateBackAsync();
}

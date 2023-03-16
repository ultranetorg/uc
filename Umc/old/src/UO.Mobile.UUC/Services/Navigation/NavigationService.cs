namespace UO.Mobile.UUC.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    public Task NavigateToAsync(Type pageType)
    {
        Guard.Against.Null(pageType, nameof(pageType));

        return UUC.Navigation.NavigateToAsync(pageType);
    }

    public Task NavigateToAsync<T>() where T : Page => NavigateToAsync<T>(null);

    public Task NavigateToAsync<T>(object parameter) where T : Page => UUC.Navigation.NavigateToAsync<T>(parameter);

    public Task NavigateBackAsync() => UUC.Navigation.NavigateBackAsync();
}

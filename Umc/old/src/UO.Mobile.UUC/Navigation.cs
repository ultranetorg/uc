using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC;

internal static class Navigation
{
    public static Task NavigateToAsync<T>() where T : Page => NavigateToAsync(typeof(T), null);

    public static Task NavigateToAsync<T>(object parameter) => NavigateToAsync(typeof(T), parameter);

    public static Task NavigateToAsync([NotNull] Type pageType)
    {
        Guard.Against.Null(pageType, nameof(pageType));

        return NavigateToAsync(pageType, null);
    }

    public static Task NavigateToAsync([NotNull] Type pageType, object parameter)
    {
        Guard.Against.Null(pageType, nameof(pageType));

        return InternalNavigateToAsync(pageType, parameter);
    }

    public static async Task NavigateBackAsync()
    {
        if (Application.Current.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }

    private static async Task InternalNavigateToAsync(Type pageType, object parameter)
    {
        Page newPage = await CreatePageAndBindContextAsync(pageType, parameter);
        await NavigateToPageAsync(newPage);
    }

    private static async Task<Page> CreatePageAndBindContextAsync(Type pageType, object parameter)
    {
        Page newPage = CreatePage(pageType);
        if (newPage.BindingContext == null)
        {
            Type viewModelType = GetViewModelTypeForPage(pageType);
            if (viewModelType != null)
            {
                object viewModel = DependencyInjection.Resolve(viewModelType);
                await TryToInitializeViewModelAsync(viewModelType, viewModel, parameter);

                newPage.BindingContext = viewModel;
            }
        }

        return newPage;
    }

    private static async Task NavigateToPageAsync([NotNull] Page page)
    {
        if (Application.Current.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushAsync(page);
        }
        else
        {
            Application.Current.MainPage = new NavigationPage(page);
        }
    }

    private static Page CreatePage(Type pageType)
    {
        return (Page) DependencyInjection.Resolve(pageType);
    }

    private static Type GetViewModelTypeForPage(Type pageType)
    {
        string viewModelName = pageType.FullName.Replace("Page", "ViewModel");
        return Type.GetType($"{viewModelName}, {typeof(ViewModels._).Assembly.FullName}");
    }

    private static async Task TryToInitializeViewModelAsync(Type viewModelType, object viewModel, object parameter)
    {
        if (viewModelType.GetInterfaces().Contains(typeof(IInitializableAsync)))
        {
            IInitializableAsync initializable = (IInitializableAsync) viewModel;
            await initializable.InitializeAsync(parameter);
        }
    }
}

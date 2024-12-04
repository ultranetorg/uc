namespace Uuc.Services;

public interface INavigationService
{
	void Push<T>(T page, IDictionary<string, object>? routeParameters = null) where T : Page;

	void Pop();
}

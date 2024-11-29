using System.Diagnostics.CodeAnalysis;

namespace Uuc.Services;

public interface INavigationService
{
	// Task InitializeAsync();

	Task NavigateToAsync([NotEmpty] string route, IDictionary<string, object>? routeParameters = null);

	Task PopAsync();
}

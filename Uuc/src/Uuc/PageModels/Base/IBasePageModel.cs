using CommunityToolkit.Mvvm.Input;
using Uuc.Services;

namespace Uuc.PageModels.Base;

public interface IBasePageModel : IQueryAttributable
{
	public INavigationService NavigationService { get; }

	public IAsyncRelayCommand InitializeAsyncCommand { get; }

	public bool IsBusy { get; }

	public bool IsInitialized { get; }

	Task InitializeAsync();
}

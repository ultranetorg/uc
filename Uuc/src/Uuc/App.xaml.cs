using Uuc.Pages;
using Uuc.Services;

namespace Uuc;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ISessionService _sessionService;
	private readonly IPasswordService _passwordService;

	public App
	(
		IServiceProvider serviceProvider,
		ISessionService sessionService,
		IPasswordService passwordService
	)
	{
		_serviceProvider = serviceProvider;
		_sessionService = sessionService;
		_passwordService = passwordService;

		InitializeComponent();

		_passwordService.Password = null;
		_sessionService.SessionExpired += OnSessionExpired;
	}

	private void OnSessionExpired()
	{
		_passwordService.Password = null;
		Page enterPasswordPage = _serviceProvider.GetRequiredService<EnterPasswordPage>();
		MainThread.BeginInvokeOnMainThread(() => Windows[0].Page = new NavigationPage(enterPasswordPage));
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Page loadingPage = _serviceProvider.GetRequiredService<LoadingPage>();
		Window window = new(loadingPage);

		InitializeWindow(window);

		return window;
	}

	private async void InitializeWindow(Window window)
	{
		Page createPasswordPage = _serviceProvider.GetRequiredService<CreatePasswordPage>();
		Page enterPasswordPage = _serviceProvider.GetRequiredService<EnterPasswordPage>();

		bool isPasswordStored = await _passwordService.IsHashSavedAsync();
		MainThread.BeginInvokeOnMainThread(() =>
		{
			Page nextPage = isPasswordStored ? enterPasswordPage : createPasswordPage;
			window.Page = new NavigationPage(nextPage);
		});
	}
}

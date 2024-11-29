using Uuc.Pages;
using Uuc.Services;

namespace Uuc;

public partial class App : Application
{
	private readonly ISessionService _sessionService;
	private readonly Shell _appShell;

	public App(ISessionService sessionService, INavigationService navigationService, AppShell appShell)
	{
		_sessionService = sessionService;
		_appShell = appShell;

		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_appShell);
	}
}
